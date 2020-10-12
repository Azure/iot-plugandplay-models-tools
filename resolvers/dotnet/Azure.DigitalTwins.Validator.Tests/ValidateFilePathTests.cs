using NUnit.Framework;

namespace Azure.DigitalTwins.Validator.Tests
{
    public class ValidateFilePathTests
    {
        [Test]
        public void should_pass_valid_filename() {
/*'dtmi/com/example/thermostat-1.json',
            'dtmi/azure/devicemanagement/deviceinformation-1.json'*/
        }

        [Test]
        public void should_fail_upper_casing() {
/*'dtmi/com/example/Thermostat-1.json',
            'dtmi/azure/devicemanagement/Deviceinformation-1.json'*/
        }

        [Test]
        public void should_fail_missing_version() {
/*'dtmi/com/example/thermostat.json',
            'dtmi/azure/devicemanagement/deviceinformation.json'*/
        }

        [Test]
        public void should_fail_missing_dtmi_folder() {
/*'com/example/thermostat-1.json',
            'azure/devicemanagement/deviceinformation-1.json'*/
        }
    }
}
