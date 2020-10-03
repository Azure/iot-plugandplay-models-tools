using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver
{
    public class ResolverClient
    {
        readonly RegistryHandler registryHandler = null;

        public static ResolverClient FromRemoteRegistry(string registryUri, ILogger logger = null)
        {
            return new ResolverClient(new Uri(registryUri), logger);
        }

        public static ResolverClient FromLocalRegistry(string registryPath, ILogger logger = null)
        {
            return new ResolverClient(new Uri($"file://{Path.GetFullPath(registryPath)}"), logger);
        }

        public ResolverClient(Uri registryUri, ILogger logger = null)
        {
            this.registryHandler = new RegistryHandler(registryUri, logger);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(string dtmi)
        {
            return await this.registryHandler.ProcessAsync(dtmi, true);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(params string[] dtmis)
        {
            return await this.registryHandler.ProcessAsync(dtmis, true);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(IEnumerable<string> dtmis)
        {
            return await this.registryHandler.ProcessAsync(dtmis, true);
        }

        public Uri RegistryUri { get { return this.registryHandler.RegistryUri; } }
    }
}
