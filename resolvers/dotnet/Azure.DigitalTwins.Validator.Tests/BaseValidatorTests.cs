using NUnit.Framework;
using Azure.DigitalTwins.Validator;

namespace Azure.DigitalTwins.Validator.Tests {
    public class BaseValidatorTests
    {
        [Test]
        public void should_fail_if_file_doesnt_exit()
        {

        }

        [Test]
        public void should_fail_if_empty_file(){

        }

        [Test]
        public void should_fail_on_missing_id_field() {
/*{"something": "dtmi:com:example:ThermoStat;1"}*/
        }
    }
}