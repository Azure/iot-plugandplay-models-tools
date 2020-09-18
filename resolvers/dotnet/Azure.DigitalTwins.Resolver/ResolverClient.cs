using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver
{
    public class ResolverClient
    {
        readonly RegistryHandler registryHandler = null;

        public static ResolverClient FromRemoteRegistry(string registryUri)
        {
            return new ResolverClient(new Uri(registryUri));
        }

        public static ResolverClient FromLocalRegistry(string registryPath)
        {
            return new ResolverClient(new Uri($@"file://{registryPath}"));
        }

        public ResolverClient(Uri registryUri)
        {
            this.registryHandler = new RegistryHandler(registryUri);
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
