using System;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Fetchers
{
    public interface IModelFetcher
    {
        Task<FetchResult> FetchAsync(string dtmi, Uri repositoryUri, bool expanded = false);

        string GetPath(string dtmi, Uri repositoryUri, bool expanded = false);
    }
}
