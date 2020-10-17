using NUnit.Framework;
using Azure.DigitalTwins.Validator;
using System.Text.Json;
using Azure.DigitalTwins.Validator.Exceptions;

namespace Azure.DigitalTwins.Validator.Tests
{
    public class ValidateContextTests
    {
        [Test]
        public void FailsOnMissingRootContext()
        {
             var doc = JsonDocument.Parse(@"{
                    ""something"": ""dtmi:com:example:ThermoStat;1""
                }"
            );
            Assert.False(Validations.ValidateContext(doc.RootElement));
        }
        [Test]
        public void ValidatesRootContext()
        {
             var doc = JsonDocument.Parse(@"{
                ""@context"": ""dtmi:dtdl:context;2"",
                ""@id"": ""dtmi:com:example:ThermoStat;1""
            }");
            Assert.True(Validations.ValidateContext(doc.RootElement));
        }

        [Test]
        public void FailsOnContextMissingSemicolon()
        {
             var doc = JsonDocument.Parse(@"{
                    ""@context"": ""dtmi:dtdl:context-2"",
                    ""@id"": ""dtmi:com:example:ThermoStat;1""
                }"
            );
            Assert.False(Validations.ValidateContext(doc.RootElement));
        }

        [Test]
        public void FailsOnMissingDTMIPortionOfContext()
        {
            var doc = JsonDocument.Parse(@"{
                ""@context"": ""dtdl:context;2"",
                ""@id"": ""dtmi:com:example:ThermoStat;1""
            }");
            Assert.False(Validations.ValidateContext(doc.RootElement));
        }
    }
}
