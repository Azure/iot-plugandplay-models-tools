using Azure.IoT.DeviceModelsRepository.Resolver;
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
            ILogger logger = LoggerFactory.Create(builder =>builder.AddConsole().SetMinimumLevel(LogLevel.Trace)).CreateLogger<Program>();
            ResolverClient rc = new ResolverClient(logger, new ResolverClientSettings(DependencyResolutionOption.Enabled));
            var models = await rc.ResolveAsync("dtmi:Advantech:EIS_D150;1");
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
