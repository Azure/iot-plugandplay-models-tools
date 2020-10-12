using NUnit.Framework;
using Azure.DigitalTwins.Validator;
using System.Text.Json;
using Azure.DigitalTwins.Validator.Exceptions;

namespace Azure.DigitalTwins.Validator.Tests
{
    public class ValidateDTMITests
    {
        [Test]
        public void should_fail_if_empty_file()
        {
            Assert.That(() => Validations.ValidateDTMI(""), Throws.Exception);
        }

        [Test]
        public void should_fail_on_missing_id_field()
        {
            Assert.Throws<MissingDTMIException>(
                () => Validations.ValidateDTMI("{\"something\": \"dtmi:com:example:ThermoStat;1\"}")
            );
        }
        [Test]
        public void should_pass_valid_DTMI()
        {
            Validations.ValidateDTMI("{\"@id\": \"dtmi:com:example:ThermoStat;1\"}");
        }

        [Test]
        public void should_fail_invalid_DTMI_missing_semicolon()
        {
            Assert.Throws<InvalidDTMIException>(
                () => Validations.ValidateDTMI("{\"@id\": \"dtmi:com:example:ThermoStat-1\"}")
            );
        }

        [Test]
        public void should_fail_invalid_DTMI_missing_dtmi()
        {
            Assert.Throws<InvalidDTMIException>(
                () => Validations.ValidateDTMI("{\"@id\": \"com:example:ThermoStat;1\"}")
            );
        }

        [Test]
        public void should_pass_valid_sub_DTMI()
        {
             Validations.ValidateDTMI("{\"@context\": \"dtmi:dtdl:context;2\"," +
                        "\"@id\": \"dtmi:com:test:device;1\"," +
                        "\"@type\": \"Interface\"," +
                        "\"displayName\": \"Microsoft Device\"," +
                        "\"contents\": [{\"@type\": \"Property\", " +
                        "  \"@id\": \"dtmi:com:test:device:property;1\", " +
                        "  \"name\": \"Failure\", " +
                        "  \"schema\": \"boolean\" }]}");
        }

        [Test]
        public void should_fail_invalid_sub_DTMI_not_namespaced()
        {
            Assert.Throws<InvalidSubDTMIException>(
                () => Validations.ValidateDTMI("{\"@context\": \"dtmi:dtdl:context;2\"," +
                        "\"@id\": \"dtmi:com:test:device;1\"," +
                        "\"@type\": \"Interface\"," +
                        "\"displayName\": \"Microsoft Device\"," +
                        "\"contents\": [{\"@type\": \"Property\", " +
                        "  \"@id\": \"dtmi:com:otherScope:property;1\", " +
                        "  \"name\": \"Failure\", " +
                        "  \"schema\": \"boolean\" }]}"));
        }

        [Test]
        public void should_fail_invalid_sub_DTMI_missing_semicolon()
        {
            Assert.Throws<InvalidDTMIException>(
                () => Validations.ValidateDTMI("{\"@context\": \"dtmi:dtdl:context;2\", " +
                        "\"@id\": \"dtmi:com:test:device;1\", " +
                        "\"@type\": \"Interface\", " +
                        "\"displayName\": \"Microsoft Device\", " +
                        "\"contents\": [{ \"@type\": \"Property\", " +
                        "  \"@id\": \"com:test:device:property;1\", " +
                        "  \"name\": \"Failure\", " +
                        "  \"schema\": \"boolean\"  } ]}"));
        }
    }
}
