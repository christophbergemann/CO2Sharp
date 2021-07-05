using System;
using System.Threading;
using System.Threading.Tasks;
using CO2SensorReader.CO2Sensor;

namespace CO2Sharp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var co2SensorController = Co2SensorController.Create();
            await Task.WhenAll(co2SensorController.Start(), DisplayData(co2SensorController));
        }

        private static async Task DisplayData(Co2SensorController co2SensorController)
        {
            while (true)
            {
                await Task.Delay(15 * 1000);
                Console.WriteLine($"Temperature: {co2SensorController.Temperature}, CO2: {co2SensorController.Co2}");
            }
        }
    }
}