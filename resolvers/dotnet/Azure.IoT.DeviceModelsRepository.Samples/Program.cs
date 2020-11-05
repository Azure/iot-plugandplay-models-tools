using Azure.Core.Diagnostics;
using Azure.IoT.DeviceModelsRepository.Resolver;
using Azure.IoT.DeviceModelsRepository.Resolver.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Samples
{
    class Program
    {

        static async Task Main(string[] args)
        {
            await ResolveAndParse();
            await ParseAndResolve();
        }

        private static async Task ResolveAndParse()
        {
            string dtmi = "dtmi:com:example:TemperatureController;1";
            using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
            ResolverClient rc = new ResolverClient();
            var models = await rc.ResolveAsync(dtmi);
            ModelParser parser = new ModelParser();
            var parseResult = await parser.ParseAsync(models.Values.ToArray());
            Console.WriteLine($"{dtmi} resolved in {models.Count} interfaces with {parseResult.Count} entities.");
        }

        private static async Task ParseAndResolve()
        {
            string dtmi = "dtmi:com:example:TemperatureController;1";
            using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();
            ResolverClient rc = new ResolverClient(new ResolverClientOptions(DependencyResolutionOption.Disabled));
            var models = await rc.ResolveAsync(dtmi);
            ModelParser parser = new ModelParser();
            parser.DtmiResolver = rc.ParserDtmiResolver;
            var parseResult = await parser.ParseAsync(models.Values.Take(1).ToArray());
            Console.WriteLine($"{dtmi} resolved in {models.Count} interfaces with {parseResult.Count} entities.");
        }
    }
}
