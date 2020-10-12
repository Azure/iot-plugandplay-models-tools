using NUnit.Framework;
using Azure.DigitalTwins.Validator;
using System.Text.Json;
using Azure.DigitalTwins.Validator.Exceptions;

namespace Azure.DigitalTwins.Validator.Tests
{
    public class ValidateContextTests
    {
        [Test]
        public void FailsOnEmptyFile()
        {
            Assert.That(() => Validations.ValidateContext(""), Throws.Exception);
        }

        [Test]
        public void FailsOnMissingRootContext()
        {
            Assert.Throws<MissingContextException>(
                () => Validations.ValidateContext("{\"something\": \"dtmi:com:example:ThermoStat;1\"}")
            );
        }
        [Test]
        public void ValidatesRootContext()
        {
            Validations.ValidateContext("{\"@context\": \"dtmi:dtdl:context;2\", \"@id\": \"dtmi:com:example:ThermoStat;1\"}");
        }

        [Test]
        public void FailsOnContextMissingSemicolon()
        {
            Assert.Throws<InvalidContextException>(
                () => Validations.ValidateContext("{\"@context\": \"dtmi:dtdl:context-2\", \"@id\": \"dtmi:com:example:ThermoStat;1\"}")
            );
        }

        [Test]
        public void FailsOnMissingDTMIPortionOfContext()
        {
            Assert.Throws<InvalidContextException>(
                () => Validations.ValidateContext("{\"@context\": \"dtdl:context;2\", \"@id\": \"dtmi:com:example:ThermoStat;1\"}")
            );
        }
    }
}
