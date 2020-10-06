using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver
{
    public class ResolverClient
    {
        readonly RepoHandler repoHandler = null;

        public static ResolverClient FromRemoteRepo(string repositoryUri, ILogger logger = null)
        {
            return new ResolverClient(new Uri(repositoryUri), logger);
        }

        public static ResolverClient FromLocalRepo(string repositoryPath, ILogger logger = null)
        {
            repositoryPath = Path.GetFullPath(repositoryPath);
            return new ResolverClient(new Uri($"file://{repositoryPath}"), logger);
        }

        public ResolverClient(Uri repositoryUri, ILogger logger = null)
        {
            this.repoHandler = new RepoHandler(repositoryUri, logger);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(string dtmi)
        {
            return await this.repoHandler.ProcessAsync(dtmi, true);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(params string[] dtmis)
        {
            return await this.repoHandler.ProcessAsync(dtmis, true);
        }

        public async Task<IDictionary<string, string>> ResolveAsync(IEnumerable<string> dtmis)
        {
            return await this.repoHandler.ProcessAsync(dtmis, true);
        }

        public string GetPath(string dtmi)
        {
            return this.repoHandler.ToPath(dtmi);
        }

        public static bool IsValidDtmi(string dtmi)
        {
            return RepoHandler.IsValidDtmi(dtmi);
        }

        public Uri RepositoryUri { get { return this.repoHandler.RepositoryUri; } }
    }
}
