using NUnit.Framework;
using Azure.IoT.DeviceModelsRepository.CLI;

namespace Azure.IoT.DeviceModelsRepository.Validation.Tests
{
    public class ValidateFilePathTests
    {
        [Test]
        public void ValidatesFilename()
        {
            Assert.True(Validations.ValidateFilePath("dtmi/com/example/thermostat-1.json"));
            Assert.True(Validations.ValidateFilePath("dtmi\\azure\\devicemanagement\\deviceinformation-1.json"));
        }

        [Test]
        public void FailsOnUppercasing()
        {
            Assert.False(Validations.ValidateFilePath("dtmi\\com\\example\\Thermostat-1.json"));
            Assert.False(Validations.ValidateFilePath("dtmi/azure/devicemanagement/Deviceinformation-1.json"));
        }

        [Test]
        public void FailsOnMissingVersion()
        {
            Assert.False(Validations.ValidateFilePath("dtmi/com/example/thermostat.json"));
            Assert.False(Validations.ValidateFilePath("dtmi/azure/devicemanagement/deviceinformation.json"));
        }

        [Test]
        public void FailsOnMissingDTMIFolder()
        {
            Assert.False(Validations.ValidateFilePath("com/example/thermostat-1.json"));
            Assert.False(Validations.ValidateFilePath("azure/devicemanagement/deviceinformation-1.json"));
        }
    }
}
