using NUnit.Framework;
using Azure.IoT.DeviceModelsRepository.CLI;

namespace Azure.IoT.DeviceModelsRepository.Validation.Tests
{
    public class IsValidDtmiPathTests
    {
        [Test]
        public void ValidatesFilename()
        {
            Assert.True(Validations.IsValidDtmiPath("dtmi/com/example/thermostat-1.json"));
            Assert.True(Validations.IsValidDtmiPath("dtmi\\azure\\devicemanagement\\deviceinformation-1.json"));
        }

        [Test]
        public void FailsOnUppercasing()
        {
            Assert.False(Validations.IsValidDtmiPath("dtmi\\com\\example\\Thermostat-1.json"));
            Assert.False(Validations.IsValidDtmiPath("dtmi/azure/devicemanagement/Deviceinformation-1.json"));
        }

        [Test]
        public void FailsOnMissingVersion()
        {
            Assert.False(Validations.IsValidDtmiPath("dtmi/com/example/thermostat.json"));
            Assert.False(Validations.IsValidDtmiPath("dtmi/azure/devicemanagement/deviceinformation.json"));
        }

        [Test]
        public void FailsOnMissingDTMIFolder()
        {
            Assert.False(Validations.IsValidDtmiPath("com/example/thermostat-1.json"));
            Assert.False(Validations.IsValidDtmiPath("azure/devicemanagement/deviceinformation-1.json"));
        }
    }
}
