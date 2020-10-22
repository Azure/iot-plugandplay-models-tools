using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    public class ResolverClient: IResolverClient
    {
        public const string DefaultRepository = "https://devicemodels.azure.com";
        private readonly RepositoryHandler repositoryHandler = null;

        public ResolverClient() : this(new Uri(DefaultRepository), null, null) { }

        public ResolverClient(Uri repositoryUri): this(repositoryUri, null, null) { }

        public ResolverClient(Uri repositoryUri, ResolverClientOptions options): this(repositoryUri, options, null) { }

        public ResolverClient(Uri repositoryUri, ResolverClientOptions options = null, ILogger logger = null)
        {
            this.repositoryHandler = new RepositoryHandler(repositoryUri, options, logger);
        }

        public ResolverClient(string repositoryUriStr) : this(repositoryUriStr, null, null) { }

        public ResolverClient(string repositoryUriStr, ResolverClientOptions options) : 
            this(repositoryUriStr, options, null) { }

        public ResolverClient(string repositoryUriStr, ResolverClientOptions options = null, ILogger logger = null) : 
            this(new Uri(repositoryUriStr), options, logger) { }

        public virtual async Task<IDictionary<string, string>> ResolveAsync(string dtmi)
        {
            return await this.repositoryHandler.ProcessAsync(dtmi);
        }

        public virtual async Task<IDictionary<string, string>> ResolveAsync(IEnumerable<string> dtmis)
        {
            return await this.repositoryHandler.ProcessAsync(dtmis);
        }

        public virtual string GetPath(string dtmi) => repositoryHandler.ToPath(dtmi);

        public static bool IsValidDtmi(string dtmi) => DtmiConventions.IsDtmi(dtmi);

        public Uri RepositoryUri  => repositoryHandler.RepositoryUri;

        public ResolverClientOptions Settings => repositoryHandler.Settings;
    }
}
