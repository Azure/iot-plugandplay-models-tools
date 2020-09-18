using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.DigitalTwins.Resolver.Fetchers
{
    public interface IModelFetcher
    {
        Task<string> Fetch(string dtmi, Uri registryUri);
    }
}
