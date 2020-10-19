using Azure.DigitalTwins.Resolver.Fetchers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver
{
    public class RepositoryHandler
    {
        private readonly IModelFetcher _modelFetcher;
        private readonly ILogger _logger;

        public enum RepositoryTypeCategory
        {
            RemoteUri,
            LocalUri
        }

        public Uri RepositoryUri { get; }
        public ResolverClientSettings Settings { get; }
        public RepositoryTypeCategory RepositoryType { get; }

        public RepositoryHandler(Uri repositoryUri, ILogger logger = null, ResolverClientSettings settings = null)
        {
            _logger = logger ?? NullLogger.Instance;
            Settings = settings ?? new ResolverClientSettings();
            RepositoryUri = repositoryUri;

            _logger.LogTrace(StandardStrings.ClientInitWithFetcher(repositoryUri.Scheme));

            if (repositoryUri.Scheme == "file")
            {
                RepositoryType = RepositoryTypeCategory.LocalUri;
                _modelFetcher = new LocalModelFetcher(_logger);
            }
            else
            {
                RepositoryType = RepositoryTypeCategory.RemoteUri;
                _modelFetcher = new RemoteModelFetcher(_logger);
            }
        }

        public string ToPath(string dtmi)
        {
            if (!IsValidDtmi(dtmi))
            {
                string invalidArgMsg = StandardStrings.InvalidDtmiFormat(dtmi);
                _logger.LogError(invalidArgMsg);
                throw new ResolverException(dtmi, invalidArgMsg, new ArgumentException(invalidArgMsg));
            }

            return _modelFetcher.GetPath(dtmi, this.RepositoryUri);
        }

        public static bool IsValidDtmi(string dtmi)
        {
            // Regex defined at https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
            Regex rx = new Regex(@"^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
            return rx.IsMatch(dtmi);
        }

        public async Task<IDictionary<string, string>> ProcessAsync(string dtmi)
        {
            return await this.ProcessAsync(new List<string>() { dtmi });
        }

        public async Task<IDictionary<string, string>> ProcessAsync(IEnumerable<string> dtmis)
        {
            Dictionary<string, string> processedModels = new Dictionary<string, string>();
            Queue<string> toProcessModels = new Queue<string>();

            foreach (string dtmi in dtmis)
            {
                if (!IsValidDtmi(dtmi))
                {
                    string invalidArgMsg = StandardStrings.InvalidDtmiFormat(dtmi);
                    _logger.LogError(invalidArgMsg);
                    throw new ResolverException(dtmi, invalidArgMsg, new ArgumentException(invalidArgMsg));
                }

                toProcessModels.Enqueue(dtmi);
            }

            while (toProcessModels.Count != 0)
            {
                string targetDtmi = toProcessModels.Dequeue();
                if (processedModels.ContainsKey(targetDtmi))
                {
                    _logger.LogTrace(StandardStrings.SkippingPreProcessedDtmi(targetDtmi));
                    continue;
                }
                _logger.LogTrace(StandardStrings.ProcessingDtmi(targetDtmi));

                FetchResult result = await this.FetchAsync(targetDtmi);
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

                if (Settings.DependencyResolution >= DependencyResolutionOption.Enabled)
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

        private async Task<FetchResult> FetchAsync(string dtmi)
        {
            try
            {
                return await this._modelFetcher.FetchAsync(
                    dtmi, this.RepositoryUri,
                    Settings.DependencyResolution == DependencyResolutionOption.FromExpanded);
            }
            catch (Exception ex)
            {
                throw new ResolverException(dtmi, ex.Message, ex);
            }
        }
    }
}
