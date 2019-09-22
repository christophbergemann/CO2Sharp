using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HidSharp;

namespace CO2Sharp
{
    class Program
    {
        private static readonly byte[] Key = {0xc4, 0xc6, 0xc0, 0x92, 0x40, 0x23, 0xdc, 0x96};

        static void Main(string[] args)
        {
            var co2Sensor = GetCo2Sensor();
            var co2Stream = co2Sensor.Open();
            co2Stream.SetFeature(new byte[]{0x00}.Concat(Key).ToArray());

            var values=new Dictionary<int, int>();
            while (true)
            {
                var decrypted = ReadData(co2Stream);
                if (!IsDataValid(decrypted))
                {
                    continue;
                }

                var op = decrypted[0];
                var val = decrypted[1] << 8 | decrypted[2];
                values[op]=val;
                if (values.ContainsKey(0x50) && values.ContainsKey(0x42))
                {
                    Console.WriteLine($"Temperature {values[0x42]/16.0-273.15}, CO2: {values[0x50]}");
                    values.Clear();
                }
            }
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
            return decrypted[4] == 0x0d && ((decrypted[0] + decrypted[1] + decrypted[2])&0xff) == decrypted[3];
        }

        private static int[] ReadData(HidStream co2Stream)
        {
            var data=co2Stream.Read();
            var dataAsInt = data.Skip(1).Select(i => (int) i).ToArray();
            var decrypted = decrypt(dataAsInt);
            return decrypted;
        }

        private static int[] decrypt(int[] data)
        {
            var cstate = new [] {0x48, 0x74, 0x65, 0x6D, 0x70, 0x39, 0x39, 0x65};
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
    }
}