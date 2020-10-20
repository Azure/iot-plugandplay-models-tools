using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    public class ResolverClient
    {
        public const string DefaultRepository = "https://devicemodels.azure.com";
        readonly RepositoryHandler repositoryHandler = null;

        public static ResolverClient FromRemoteRepository(string repositoryUri, ILogger logger = null, ResolverClientSettings settings = null)
        {
            return new ResolverClient(new Uri(repositoryUri), logger, settings);
        }

        public static ResolverClient FromLocalRepository(string repositoryPath, ILogger logger = null, ResolverClientSettings settings = null)
        {
            repositoryPath = Path.GetFullPath(repositoryPath);
            return new ResolverClient(new Uri($"file://{repositoryPath}"), logger, settings);
        }

        public ResolverClient() : this(new Uri(DefaultRepository))
        {
        }

        public ResolverClient(string url) : this(new Uri(url))
        {
        }

        public ResolverClient(ILogger logger = null, ResolverClientSettings settings = null) : this(new Uri(DefaultRepository), logger, settings)
        {
        }

        public ResolverClient(Uri repositoryUri, ILogger logger = null, ResolverClientSettings settings = null)
        {
            this.repositoryHandler = new RepositoryHandler(repositoryUri, logger, settings);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(string dtmi)
        {
            return await this.repositoryHandler.ProcessAsync(dtmi);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(params string[] dtmis)
        {
            return await this.repositoryHandler.ProcessAsync(dtmis);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(IEnumerable<string> dtmis)
        {
            return await this.repositoryHandler.ProcessAsync(dtmis);
        }

        public string GetPath(string dtmi) => repositoryHandler.ToPath(dtmi);
        
        public static bool IsValidDtmi(string dtmi) => RepositoryHandler.IsValidDtmi(dtmi);
        

        public Uri RepositoryUri  => repositoryHandler.RepositoryUri;

        public ResolverClientSettings Settings => repositoryHandler.Settings;
    }
}
