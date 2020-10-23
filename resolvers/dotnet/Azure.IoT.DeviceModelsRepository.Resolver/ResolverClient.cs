using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    public class ResolverClient
    {
        public const string DefaultRepository = "https://devicemodels.azure.com";
        private readonly RepositoryHandler repositoryHandler = null;

        public ResolverClient() : this(new Uri(DefaultRepository), null, null) { }

        public ResolverClient(Uri repositoryUri) : this(repositoryUri, null, null) { }

        public ResolverClient(ILogger logger) : this(new Uri(DefaultRepository), null, logger) { }

        public ResolverClient(ResolverClientOptions options) : this(new Uri(DefaultRepository), options, null) { }

        public ResolverClient(ResolverClientOptions options, ILogger logger) : this(new Uri(DefaultRepository), options, logger) { }

        public ResolverClient(Uri repositoryUri, ResolverClientOptions options) : this(repositoryUri, options, null) { }

        public ResolverClient(Uri repositoryUri, ResolverClientOptions options = null, ILogger logger = null)
        {
            this.repositoryHandler = new RepositoryHandler(repositoryUri, options, logger);
        }

        public ResolverClient(string repositoryUriStr) : this(repositoryUriStr, null, null) { }

        public ResolverClient(string repositoryUriStr, ResolverClientOptions options) :
            this(repositoryUriStr, options, null)
        { }

        public ResolverClient(string repositoryUriStr, ResolverClientOptions options = null, ILogger logger = null) :
            this(new Uri(repositoryUriStr), options, logger)
        { }

        public virtual async Task<IDictionary<string, string>> ResolveAsync(string dtmi, CancellationToken cancellationToken = default)
        {
            return await this.repositoryHandler.ProcessAsync(dtmi, cancellationToken);
        }

        public virtual async Task<IDictionary<string, string>> ResolveAsync(IEnumerable<string> dtmis, CancellationToken cancellationToken = default)
        {
            return await this.repositoryHandler.ProcessAsync(dtmis, cancellationToken);
        }

        public static bool IsValidDtmi(string dtmi) => DtmiConventions.IsDtmi(dtmi);

        public Uri RepositoryUri => repositoryHandler.RepositoryUri;

        public ResolverClientOptions ClientOptions => repositoryHandler.ClientOptions;
    }
}
