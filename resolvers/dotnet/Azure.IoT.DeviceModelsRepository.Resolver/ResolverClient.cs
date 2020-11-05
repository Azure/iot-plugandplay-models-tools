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

        public ResolverClient() : this(new Uri(DefaultRepository), null) { }

        public ResolverClient(Uri repositoryUri) : this(repositoryUri, null) { }

        public ResolverClient(ResolverClientOptions options) : this(new Uri(DefaultRepository), options) { }

        public ResolverClient(Uri repositoryUri, ResolverClientOptions options)
        {
            this.repositoryHandler = new RepositoryHandler(repositoryUri, options);
        }

        public ResolverClient(string repositoryUriStr) : this(repositoryUriStr, null) { }

        public ResolverClient(string repositoryUriStr, ResolverClientOptions options) :
            this(new Uri(repositoryUriStr), options)
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
