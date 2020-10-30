using Azure.IoT.DeviceModelsRepository.Resolver.Fetchers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    internal class RepositoryHandler
    {
        private readonly IModelFetcher _modelFetcher;
        private readonly ILogger _logger;

        public enum RepositoryTypeCategory
        {
            RemoteUri,
            LocalUri
        }

        public Uri RepositoryUri { get; }
        public ResolverClientOptions ClientOptions { get; }
        public RepositoryTypeCategory RepositoryType { get; }

        public RepositoryHandler(Uri repositoryUri, ResolverClientOptions options = null, ILogger logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
            ClientOptions = options ?? new ResolverClientOptions();
            RepositoryUri = repositoryUri;

            _logger.LogTrace(StandardStrings.ClientInitWithFetcher(repositoryUri.Scheme));

            if (repositoryUri.Scheme == "file")
            {
                RepositoryType = RepositoryTypeCategory.LocalUri;
                _modelFetcher = new LocalModelFetcher(_logger, ClientOptions);
            }
            else
            {
                RepositoryType = RepositoryTypeCategory.RemoteUri;
                _modelFetcher = new RemoteModelFetcher(_logger, ClientOptions);
            }
        }

        public async Task<IDictionary<string, string>> ProcessAsync(string dtmi, CancellationToken cancellationToken)
        {
            return await this.ProcessAsync(new List<string>() { dtmi }, cancellationToken);
        }

        public async Task<IDictionary<string, string>> ProcessAsync(IEnumerable<string> dtmis, CancellationToken cancellationToken)
        {
            Dictionary<string, string> processedModels = new Dictionary<string, string>();
            Queue<string> toProcessModels = new Queue<string>();

            foreach (string dtmi in dtmis)
            {
                if (!DtmiConventions.IsDtmi(dtmi))
                {
                    string invalidArgMsg = StandardStrings.InvalidDtmiFormat(dtmi);
                    _logger.LogError(invalidArgMsg);
                    throw new ResolverException(dtmi, invalidArgMsg, new ArgumentException(invalidArgMsg));
                }

                toProcessModels.Enqueue(dtmi);
            }

            while (toProcessModels.Count != 0 && !cancellationToken.IsCancellationRequested)
            {
                string targetDtmi = toProcessModels.Dequeue();
                if (processedModels.ContainsKey(targetDtmi))
                {
                    _logger.LogTrace(StandardStrings.SkippingPreProcessedDtmi(targetDtmi));
                    continue;
                }
                _logger.LogTrace(StandardStrings.ProcessingDtmi(targetDtmi));

                FetchResult result = await this.FetchAsync(targetDtmi, cancellationToken);
                if (result.FromExpanded)
                {
                    Dictionary<string, string> expanded = await new ModelQuery(result.Definition).ListToDictAsync();
                    foreach (KeyValuePair<string, string> kvp in expanded)
                    {
                        if (!processedModels.ContainsKey(kvp.Key))
                            processedModels.Add(kvp.Key, kvp.Value);
                    }

                    continue;
                }

                ModelMetadata metadata = new ModelQuery(result.Definition).GetMetadata();

                if (ClientOptions.DependencyResolution >= DependencyResolutionOption.Enabled)
                {
                    IList<string> dependencies = metadata.Dependencies;

                    if (dependencies.Count > 0)
                        _logger.LogTrace(StandardStrings.DiscoveredDependencies(dependencies));

                    foreach (string dep in dependencies)
                    {
                        toProcessModels.Enqueue(dep);
                    }
                }

                string parsedDtmi = metadata.Id;
                if (!parsedDtmi.Equals(targetDtmi, StringComparison.Ordinal))
                {
                    string formatErrorMsg = StandardStrings.IncorrectDtmiCasing(targetDtmi, parsedDtmi);
                    throw new ResolverException(targetDtmi, formatErrorMsg, new FormatException(formatErrorMsg));
                }

                processedModels.Add(targetDtmi, result.Definition);
            }

            return processedModels;
        }

        private async Task<FetchResult> FetchAsync(string dtmi, CancellationToken cancellationToken)
        {
            try
            {
                return await this._modelFetcher.FetchAsync(dtmi, this.RepositoryUri, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ResolverException(dtmi, ex.Message, ex);
            }
        }
    }
}
