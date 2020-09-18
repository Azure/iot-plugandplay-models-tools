using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Azure.DigitalTwins.Resolver.Fetchers
{
    public class RemoteModelFetcher : IModelFetcher
    {
        static readonly HttpClient httpClient;

        static RemoteModelFetcher()
        {
            // HttpClient is intended to be instantiated once per application, rather than per-use.
            httpClient = new HttpClient();
        }

        public async Task<string> Fetch(string dtmi, Uri registryUri)
        {
            string absoluteUri = registryUri.AbsoluteUri;
            string dtmiRemotePath = Utility.DtmiToRemotePath(dtmi, absoluteUri);

            HttpResponseMessage response = await httpClient.GetAsync(dtmiRemotePath);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
