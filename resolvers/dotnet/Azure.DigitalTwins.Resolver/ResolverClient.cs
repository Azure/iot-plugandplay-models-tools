using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver
{
    public class ResolverClient
    {
        readonly RepositoryHandler repositoryHandler = null;

        public static ResolverClient FromRemoteRepository(string repositoryUri, ILogger logger = null, ResolutionSettings settings = null)
        {
            return new ResolverClient(new Uri(repositoryUri), logger, settings);
        }

        public static ResolverClient FromLocalRepository(string repositoryPath, ILogger logger = null, ResolutionSettings settings = null)
        {
            repositoryPath = Path.GetFullPath(repositoryPath);
            return new ResolverClient(new Uri($"file://{repositoryPath}"), logger, settings);
        }

        public ResolverClient(Uri repositoryUri, ILogger logger = null, ResolutionSettings settings = null)
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

        public string GetPath(string dtmi)
        {
            return this.repositoryHandler.ToPath(dtmi);
        }

        public static bool IsValidDtmi(string dtmi)
        {
            return RepositoryHandler.IsValidDtmi(dtmi);
        }

        public Uri RepositoryUri { get { return this.repositoryHandler.RepositoryUri; } }

        public ResolutionSettings Settings { get { return this.repositoryHandler.Settings; } }
    }
}
