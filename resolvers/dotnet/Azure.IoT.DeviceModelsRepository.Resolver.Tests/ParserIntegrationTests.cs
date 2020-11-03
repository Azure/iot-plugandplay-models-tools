using Azure.IoT.DeviceModelsRepository.Resolver.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Tests
{
    public class ParserIntegrationTests
    {
        [Test]
        public async Task ParserValidationResolveFromLocalRepository()
        {
            ModelParser parser = new ModelParser();
            string TestRegistryPath = TestHelpers.TestLocalModelRepository;

            List<string> parseModelPaths = new List<string>()
            {
                $"{TestRegistryPath}/dtmi/company/demodevice-2.json",
                $"{TestRegistryPath}/dtmi/com/example/temperaturecontroller-1.json",
                $"{TestRegistryPath}/dtmi/com/example/camera-3.json",
            };

            // Shows how to quickly integrate the resolver client with the parser.
            ResolverClient client = new ResolverClient(TestRegistryPath);
            parser.DtmiResolver = client.ParserDtmiResolver;

            foreach(string modelPath in parseModelPaths) {
                // Parser will throw on validation errors
                try {
                    await parser.ParseAsync(new string[] { File.ReadAllText(modelPath) });
                }
                catch (Exception e)
                {
                    Assert.Fail(e.Message);
                }
            }
        }

        [Test]
        public void ParserValidationResolveFromLocalRepoErrorOnParserCallbackDtmiCasing()
        {
            ModelParser parser = new ModelParser();
            string TestRegistryPath = TestHelpers.TestLocalModelRepository;

            // This model references another model with invalid casing.
            string modelPath = $"{TestRegistryPath}/dtmi/company/demodevice-1.json";

            // Shows how to quickly integrate the resolver client with the parser.
            ResolverClient client = new ResolverClient(TestRegistryPath);
            parser.DtmiResolver = client.ParserDtmiResolver;

            // Parser will throw on validation errors

            ResolverException e = 
                Assert.ThrowsAsync<ResolverException>(async () => await parser.ParseAsync(new string[] { File.ReadAllText(modelPath) }));

            Assert.AreEqual(e.Message,
                $"{StandardStrings.GenericResolverError("dtmi:azure:deviceManagement:DeviceInformation;1")}" +
                $"{StandardStrings.IncorrectDtmiCasing("dtmi:azure:deviceManagement:DeviceInformation;1","dtmi:azure:DeviceManagement:DeviceInformation;1")}");
        }

        [Test]
        public async Task ParserValidationResolveFromRemoteRepository()
        {
            ModelParser parser = new ModelParser();

            // TODO: One off model -- need consistent remote model repo for IT's
            string TestRepoPath = TestHelpers.TestLocalModelRepository;
            string testModelPath = $"{TestRepoPath}/dtmi/company/demodevice-2.json";

            // Shows how to quickly integrate the resolver client with the parser.
            ResolverClient client = new ResolverClient(TestHelpers.TestRemoteModelRepository);
            parser.DtmiResolver = client.ParserDtmiResolver;

            // Parser will throw on validation errors
            await parser.ParseAsync(new string[] { File.ReadAllText(testModelPath) });
        }
    }
}
