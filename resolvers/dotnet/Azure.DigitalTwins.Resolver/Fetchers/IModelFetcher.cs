using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver.Fetchers
{
    public interface IModelFetcher
    {
        Task<string> FetchAsync(string dtmi, Uri registryUri, ClientLogger logger);
    }
}
