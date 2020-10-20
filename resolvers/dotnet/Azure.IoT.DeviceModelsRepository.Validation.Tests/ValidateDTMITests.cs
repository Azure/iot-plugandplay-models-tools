using NUnit.Framework;
using System.Text.Json;
using Azure.IoT.DeviceModelsRepository.CLI.Exceptions;
using Azure.IoT.DeviceModelsRepository.CLI;

namespace Azure.IoT.DeviceModelsRepository.Validation.Tests
{
    public class ValidateDTMITests
    {
        [Test]
        public void FailsOnMissingRootId()
        {
            var doc = JsonDocument.Parse(@"{
                ""something"": ""dtmi:com:example:ThermoStat;1""
            }");
            Assert.Throws<MissingDTMIException>(() => Validations.ValidateDTMIs(doc.RootElement));
        }
        [Test]
        public void ValidatesRootId()
        {
            var doc = JsonDocument.Parse(@"{
                ""@id"": ""dtmi:com:example:ThermoStat;1""
            }");
            Assert.True(Validations.ValidateDTMIs(doc.RootElement));
        }

        [Test]
        public void FailsOnRootIdMissingSemicolon()
        {
            var doc = JsonDocument.Parse(@"{
                ""@id"": ""dtmi:com:example:ThermoStat-1""
            }");
            Assert.False(Validations.ValidateDTMIs(doc.RootElement));
        }

        [Test]
        public void FailsOnMissingDTMIPortionOfRootId()
        {
            var doc = JsonDocument.Parse(@"{
                ""@id"": ""com:example:ThermoStat;1""
            }");
            Assert.False(Validations.ValidateDTMIs(doc.RootElement));
        }

        [Test]
        public void ValidatesSubDTMI()
        {
            var doc = JsonDocument.Parse(@"{
                ""@context"": ""dtmi:dtdl:context;2"",
                ""@id"": ""dtmi:com:test:device;1"",
                ""@type"": ""Interface"",
                ""displayName"": ""Microsoft Device"",
                ""contents"": [
                    {
                        ""@type"": ""Property"",
                        ""@id"": ""dtmi:com:test:device:property;1"",
                        ""name"": ""Failure"",
                        ""schema"": ""boolean""
                    }
                ]
            }");
            Assert.True(Validations.ValidateDTMIs(doc.RootElement));
        }

        [Test]
        public void FailsOnSubDTMIThatAreNotNamespaced()
        {
            var doc = JsonDocument.Parse(@"{
                ""@context"": ""dtmi:dtdl:context;2"",
                ""@id"": ""dtmi:com:test:device;1"",
                ""@type"": ""Interface"",
                ""displayName"": ""Microsoft Device"",
                ""contents"": [
                    {
                        ""@type"": ""Property"",
                        ""@id"": ""dtmi:com:otherScope:property;1"",
                        ""name"": ""Failure"",
                        ""schema"": ""boolean""
                    }
                ]
            }");
            Assert.False(Validations.ValidateDTMIs(doc.RootElement));
        }

        [Test]
        public void FailsOnSubDTMIWithInvalidFormats()
        {
            var doc = JsonDocument.Parse(@"{
                ""@context"": ""dtmi:dtdl:context;2"",
                ""@id"": ""dtmi:com:test:device;1"",
                ""@type"": ""Interface"",
                ""displayName"": ""Microsoft Device"",
                ""contents"": [
                    {
                        ""@type"": ""Property"",
                        ""@id"": ""com:test:device:property;1"",
                        ""name"": ""Failure"",
                        ""schema"": ""boolean""
                    }
                ]
            }");
            Assert.False(Validations.ValidateDTMIs(doc.RootElement));
        }
    }
}
