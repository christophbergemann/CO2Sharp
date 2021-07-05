using System;
using System.Linq;
using System.Threading.Tasks;
using HidSharp;

namespace CO2SensorReader.CO2Sensor
{
    public class Co2SensorController : IDisposable
    {
        private HidStream _stream;
        private static readonly byte[] Key = {0xc4, 0xc6, 0xc0, 0x92, 0x40, 0x23, 0xdc, 0x96};
        private volatile float _temperature = -999;
        private volatile float _co2Value = -999;
        private volatile bool _shouldRun = true;

        public float Temperature => _temperature;
        public float Co2 => _co2Value;

        private Co2SensorController()
        {
        }

        public static Co2SensorController Create()
        {
            var co2Sensor = GetCo2Sensor();
            var co2Stream = co2Sensor.Open();
            co2Stream.SetFeature(new byte[] {0x00}.Concat(Key).ToArray());

            return new Co2SensorController
            {
                _stream = co2Stream
            };
        }

        public Task Start()
        {
            var task = new Task(() =>
            {
                while (_shouldRun)
                {
                    byte[] data;
                    try
                    {
                        data = _stream.Read();
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    var dataAsInt = data.Skip(1).Select(i => (int) i).ToArray();
                    var decrypted = DecryptDeviceData(dataAsInt);
                    if (!IsDataValid(decrypted))
                    {
                        continue;
                    }

                    var operation = decrypted[0];
                    var measurementResult = decrypted[1] << 8 | decrypted[2];
                    switch (operation)
                    {
                        case 0x42:
                            _temperature = measurementResult / 16.0f - 273.15f;
                            break;
                        case 0x50:
                            _co2Value = measurementResult;
                            break;
                    }
                }
            });
            task.Start();
            return task;
        }

        public void Stop()
        {
            _shouldRun = false;
        }

        private static HidDevice GetCo2Sensor()
        {
            var deviceList = DeviceList.Local;
            var hidDevices = deviceList.GetHidDevices().ToList();
            var co2Sensor = hidDevices.Single(x => x.ProductID == 0xa052 && x.VendorID == 0x04d9);
            return co2Sensor;
        }

        private static bool IsDataValid(int[] decrypted)
        {
            return decrypted[4] == 0x0d && ((decrypted[0] + decrypted[1] + decrypted[2]) & 0xff) == decrypted[3];
        }

        public static int[] DecryptDeviceData(int[] data)
        {
            var cstate = new[] {0x48, 0x74, 0x65, 0x6D, 0x70, 0x39, 0x39, 0x65};
            var shuffle = new[] {2, 4, 0, 7, 1, 6, 5, 3};

            var phase1 = new[] {0, 0, 0, 0, 0, 0, 0, 0};
            for (var i = 0; i < 8; i++)
            {
                phase1[i] = data[shuffle[i]];
            }

            var phase2 = new[] {0, 0, 0, 0, 0, 0, 0, 0};
            for (var i = 0; i < 8; i++)
            {
                phase2[i] = phase1[i] ^ Key[i];
            }

            var phase3 = new[] {0, 0, 0, 0, 0, 0, 0, 0};
            for (var i = 0; i < 8; i++)
            {
                phase3[i] = (phase2[i] >> 3) | (phase2[(i - 1 + 8) % 8] << 5) & 0xff;
            }

            var ctmp = new[] {0, 0, 0, 0, 0, 0, 0, 0};
            for (var i = 0; i < 8; i++)
            {
                ctmp[i] = (cstate[i] >> 4) | (cstate[i] << 4) & 0xff;
            }

            var result = new[] {0, 0, 0, 0, 0, 0, 0, 0};
            for (var i = 0; i < 8; i++)
            {
                result[i] = (0x100 + phase3[i] - ctmp[i]) & 0xff;
            }

            return result;
        }

        public void Dispose()
        {
            Stop();
            _stream?.Dispose();
        }
    }
}