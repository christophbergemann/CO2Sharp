using System;
using System.Threading;
using CO2SensorReader.CO2Sensor;

namespace CO2Sharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var co2SensorController = Co2SensorController.Create();
            co2SensorController.Start();
            while (true)
            {
                Thread.Sleep(15 * 1000);
                Console.WriteLine($"Temperature: {co2SensorController.Temperature}, CO2: {co2SensorController.Co2}");
            }
        }
    }
}