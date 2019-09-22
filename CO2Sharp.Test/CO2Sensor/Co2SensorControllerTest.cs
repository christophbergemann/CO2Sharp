using CO2Sharp.CO2Sensor;
using FluentAssertions;
using Xunit;

namespace CO2Sharp.Test.CO2Sensor
{
    public class Co2SensorControllerTest
    {
        [Fact]
        public void Decrypt_should_return_correct_values()
        {
            var testdata = new[] {28, 228, 95, 32, 218, 70, 191, 162};
            var expected = new[] {79, 28, 69, 176, 13, 0, 0, 0};
            
            var result = Co2SensorController.DecryptDeviceData(testdata);
            
            result.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        }
    }
}