using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Azure.DigitalTwins.Resolver.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Azure.DigitalTwins.Resolver.Tests
{
    public class ParserIntegrationTests
    {
        [Test]
        public async Task ParserValidationResolveFromLocalRegistry()
        {
            ModelParser parser = new ModelParser
            {
                Options = new HashSet<ModelParsingOption>() { ModelParsingOption.StrictPartitionEnforcement }
            };

            string TestRegistryPath = TestHelpers.GetTestLocalModelRegistry();
            string testModelPath = $@"{TestRegistryPath}/dtmi/company/demodevice-1.json";

            // Shows how to quickly integrate the resolver client with the parser.
            ResolverClient client = ResolverClient.FromLocalRegistry(TestRegistryPath);
            parser.DtmiResolver = client.ParserDtmiResolver;

            // Parser will throw on validation errors
            await parser.ParseAsync(new string[] { File.ReadAllText(testModelPath) });
        }

        [Test]
        public async Task ParserValidationResolveFromRemoteRegistry()
        {
            ModelParser parser = new ModelParser
            {
                Options = new HashSet<ModelParsingOption>() { ModelParsingOption.StrictPartitionEnforcement }
            };

            // TODO: One off model -- need consistent remote model registry for IT's
            string TestRegistryPath = TestHelpers.GetTestLocalModelRegistry();
            string testModelPath = $@"{TestRegistryPath}/dtmi/company/demodevice-2.json";

            // Shows how to quickly integrate the resolver client with the parser.
            ResolverClient client = ResolverClient.FromRemoteRegistry(TestHelpers.GetTestRemoteModelRegistry());
            parser.DtmiResolver = client.ParserDtmiResolver;

            // Parser will throw on validation errors
            await parser.ParseAsync(new string[] { File.ReadAllText(testModelPath) });
        }
    }
}
