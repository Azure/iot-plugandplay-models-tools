using NUnit.Framework;
using Azure.DigitalTwins.Validator;
using Azure.DigitalTwins.Validator.Exceptions;

namespace Azure.DigitalTwins.Validator.Tests
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
        public void ValidatesFileInfo()
        {
            var fileInfo = new System.IO.FileInfo("./TestModelRepo/dtmi/azure/devicemanagment/deviceinformation-1.json");
            Assert.True(Validations.ValidateFilePath(fileInfo.FullName));
        }

        [Test]
        public void ValidatesFileInfoWindowsSlashes()
        {
            var fileInfo = new System.IO.FileInfo(".\\TestModelRepo\\dtmi\\azure\\devicemanagement\\deviceinformation-1.json");
            Assert.True(Validations.ValidateFilePath(fileInfo.FullName));
        }
        [Test]
        public void FailsOnImproperFolderStructure()
        {
            var fileInfo = new System.IO.FileInfo(".\\badfile\\deviceinformation-1.json");
            Assert.False(Validations.ValidateFilePath(fileInfo.FullName));
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
