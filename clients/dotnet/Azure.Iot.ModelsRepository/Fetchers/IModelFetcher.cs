using System;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Iot.ModelsRepository.Fetchers
{
    public interface IModelFetcher
    {
        Task<FetchResult> FetchAsync(string dtmi, Uri repositoryUri, CancellationToken cancellationToken = default);

        FetchResult Fetch(string dtmi, Uri repositoryUri, CancellationToken cancellationToken = default);
    }
}
