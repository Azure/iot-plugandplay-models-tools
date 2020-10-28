using NUnit.Framework;
using System.Text.Json;

namespace Azure.IoT.DeviceModelsRepository.CLI.Tests
{
    public class EnsureSubDtmiNamespaceTests
    {
        [Test]
        public void ValidatesSubDTMI()
        {
            string doc = @"{
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
            }";

            var result = Validations.EnsureSubDtmiNamespace(doc);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void FailsOnSubDTMIThatAreNotNamespaced()
        {
            string doc = @"{
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
            }";

            var result = Validations.EnsureSubDtmiNamespace(doc);
            Assert.True(result.Count > 0);
        }

        [Test]
        public void FailsOnSubDTMIWithInvalidFormats()
        {
            string doc = @"{
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
            }";

            var result = Validations.EnsureSubDtmiNamespace(doc);
            Assert.True(result.Count > 0);
        }
    }
}
