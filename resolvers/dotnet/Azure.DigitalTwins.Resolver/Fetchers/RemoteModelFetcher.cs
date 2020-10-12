using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver.Fetchers
{
    public class RemoteModelFetcher : IModelFetcher
    {
        static readonly HttpClient httpClient;
        private readonly ILogger _logger;

        static RemoteModelFetcher()
        {
            // HttpClient is intended to be instantiated once per application, rather than per-use.
            httpClient = new HttpClient();
        }

        public RemoteModelFetcher(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<FetchResult> FetchAsync(string dtmi, Uri repositoryUri, bool expanded = false)
        {
            Queue<string> work = new Queue<string>();

            if (expanded)
                work.Enqueue(GetPath(dtmi, repositoryUri, true));

            work.Enqueue(GetPath(dtmi, repositoryUri, false));

            string remoteFetchError = string.Empty;
            while (work.Count != 0)
            {
                string tryContentPath = work.Dequeue();
                _logger.LogInformation(StandardStrings.FetchingContent(tryContentPath));

                string content = await EvaluatePathAsync(tryContentPath);
                if (!string.IsNullOrEmpty(content))
                {
                    return new FetchResult()
                    {
                        Definition = content,
                        Path = tryContentPath
                    };
                }

                remoteFetchError = StandardStrings.ErrorAccessRemoteRepositoryModel(tryContentPath);
                _logger.LogWarning(remoteFetchError);
            }

            throw new HttpRequestException(remoteFetchError);
        }

        public string GetPath(string dtmi, Uri registryUri, bool expanded = false)
        {
            string absoluteUri = registryUri.AbsoluteUri;
            return DtmiConventions.ToPath(dtmi, absoluteUri, expanded);
        }

        private async Task<string> EvaluatePathAsync(string path)
        {
            HttpResponseMessage response = await httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }
    }
}
