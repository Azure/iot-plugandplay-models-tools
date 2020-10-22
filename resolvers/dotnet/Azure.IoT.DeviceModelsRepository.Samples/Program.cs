using Azure.IoT.DeviceModelsRepository.Resolver;
using Azure.IoT.DeviceModelsRepository.Resolver.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ILogger logger = LoggerFactory.Create(builder =>builder.AddDebug().SetMinimumLevel(LogLevel.Trace)).CreateLogger<Program>();
            ResolverClient rc = new ResolverClient(new Uri("https://devicemodels.azure.com"), logger, new ResolverClientSettings(DependencyResolutionOption.FromExpanded));
            var models = await rc.ResolveAsync("dtmi:Advantech:EIS_D150;1");
            ModelParser parser = new ModelParser();
            
            await parser.ParseAsync(models.Values.ToArray());

            DumpModels(models);

        }

        private static void DumpModels(IDictionary<string, string> models)
        {
            Console.WriteLine($"Found {models.Count} interfaces:");
            foreach (var m in models)
            {
                Console.WriteLine(m.Key);
                Console.WriteLine(m.Value.Substring(0, 200) + " . . . ");
                Console.WriteLine();
            }
        }
    }
}
