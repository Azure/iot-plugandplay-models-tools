using Azure.IoT.DeviceModelsRepository.Resolver.Fetchers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    internal class RepositoryHandler
    {
        private readonly IModelFetcher _modelFetcher;
        private readonly Guid _clientId;

        public Uri RepositoryUri { get; }
        public ResolverClientOptions ClientOptions { get; }

        public RepositoryHandler(Uri repositoryUri, ResolverClientOptions options = null)
        {
            ClientOptions = options ?? new ResolverClientOptions();
            RepositoryUri = repositoryUri;
            _modelFetcher = repositoryUri.Scheme == "file" ?
                _modelFetcher = new LocalModelFetcher(ClientOptions) :
                _modelFetcher = new RemoteModelFetcher(ClientOptions);
            _clientId = Guid.NewGuid();
            ResolverEventSource.Shared.InitFetcher(_clientId, repositoryUri.Scheme);
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
                    ResolverEventSource.Shared.InvalidDtmiInput(dtmi);
                    string invalidArgMsg = string.Format(StandardStrings.InvalidDtmiFormat, dtmi);
                    throw new ResolverException(dtmi, invalidArgMsg, new ArgumentException(invalidArgMsg));
                }

                toProcessModels.Enqueue(dtmi);
            }

            while (toProcessModels.Count != 0 && !cancellationToken.IsCancellationRequested)
            {
                string targetDtmi = toProcessModels.Dequeue();
                if (processedModels.ContainsKey(targetDtmi))
                {
                    ResolverEventSource.Shared.SkippingPreprocessedDtmi(targetDtmi);
                    continue;
                }
                ResolverEventSource.Shared.ProcessingDtmi(targetDtmi);

                FetchResult result = await FetchAsync(targetDtmi, cancellationToken);
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
                        ResolverEventSource.Shared.DiscoveredDependencies(string.Join("\", \"", dependencies));

                    foreach (string dep in dependencies)
                    {
                        toProcessModels.Enqueue(dep);
                    }
                }

                string parsedDtmi = metadata.Id;
                if (!parsedDtmi.Equals(targetDtmi, StringComparison.Ordinal))
                {
                    ResolverEventSource.Shared.IncorrectDtmiCasing(targetDtmi, parsedDtmi);
                    string formatErrorMsg = string.Format(StandardStrings.IncorrectDtmiCasing, targetDtmi, parsedDtmi);
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
                return await _modelFetcher.FetchAsync(dtmi, this.RepositoryUri, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new ResolverException(dtmi, ex.Message, ex);
            }
        }
    }
}
