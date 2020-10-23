using System;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Fetchers
{
    public interface IModelFetcher
    {
        Task<FetchResult> FetchAsync(string dtmi, Uri repositoryUri, CancellationToken cancellationToken = default);

        FetchResult Fetch(string dtmi, Uri repositoryUri, CancellationToken cancellationToken = default);
    }
}
