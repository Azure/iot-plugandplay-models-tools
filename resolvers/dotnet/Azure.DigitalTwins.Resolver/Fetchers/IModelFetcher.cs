using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver.Fetchers
{
    public interface IModelFetcher
    {
        Task<FetchResult> FetchAsync(string dtmi, Uri repositoryUri, bool expanded = false);

        string GetPath(string dtmi, Uri repositoryUri, bool expanded = false);
    }
}
