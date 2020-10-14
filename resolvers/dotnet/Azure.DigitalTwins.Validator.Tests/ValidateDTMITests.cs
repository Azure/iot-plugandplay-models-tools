using NUnit.Framework;
using Azure.DigitalTwins.Validator;
using System.Text.Json;
using Azure.DigitalTwins.Validator.Exceptions;

namespace Azure.DigitalTwins.Validator.Tests
{
    public class ValidateDTMITests
    {
        [Test]
        public void FailsOnEmptyFile()
        {
            Assert.That(() => Validations.ValidateDTMI(""), Throws.Exception);
        }

        [Test]
        public void FailsOnMissingRootId()
        {
            Assert.Throws<MissingDTMIException>(
                () => Validations.ValidateDTMI(@"{
                    ""something"": ""dtmi:com:example:ThermoStat;1""
                }")
            );
        }
        [Test]
        public void ValidatesRootId()
        {
            Validations.ValidateDTMI(@"{
                ""@id"": ""dtmi:com:example:ThermoStat;1""
            }");
        }

        [Test]
        public void FailsOnRootIdMissingSemicolon()
        {
            Assert.Throws<InvalidDTMIException>(
                () => Validations.ValidateDTMI(@"{
                    ""@id"": ""dtmi:com:example:ThermoStat-1""
                }")
            );
        }

        [Test]
        public void FailsOnMissingDTMIPortionOfRootId()
        {
            Assert.Throws<InvalidDTMIException>(
                () => Validations.ValidateDTMI(@"{
                    ""@id"": ""com:example:ThermoStat;1""
                }")
            );
        }

        [Test]
        public void ValidatesSubDTMI()
        {
             Validations.ValidateDTMI(@"{
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
        }

        [Test]
        public void FailsOnSubDTMIThatArenNotNamespaced()
        {
            Assert.Throws<InvalidSubDTMIException>(
                () => Validations.ValidateDTMI(@"{
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
                }"));
        }

        [Test]
        public void FailsOnSubDTMIWithInvalidFormats()
        {
            Assert.Throws<InvalidDTMIException>(
                () => Validations.ValidateDTMI(@"{
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
                }"));
        }
    }
}
