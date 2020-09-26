using Azure.DigitalTwins.Resolver.Fetchers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver
{
    public class RegistryHandler
    {
        private readonly IModelFetcher _modelFetcher;
        private readonly ILogger _logger;
        private readonly ModelQuery _modelQuery;

        public enum RegistryTypeCategory
        {
            RemoteUri,
            LocalUri
        }

        public Uri RegistryUri { get; }
        public RegistryTypeCategory RegistryType { get; }

        public RegistryHandler(Uri registryUri, ILogger logger=null)
        {
            _logger = logger ?? NullLogger.Instance;
            _modelQuery = new ModelQuery();
            RegistryUri = registryUri;

            if (registryUri.Scheme == "file")
            {
                _logger.LogInformation("Client initialized with file content fetcher.");
                RegistryType = RegistryTypeCategory.LocalUri;
                _modelFetcher = new LocalModelFetcher();
            }
            else
            {
                _logger.LogInformation("Client initialized with http content fetcher.");
                RegistryType = RegistryTypeCategory.RemoteUri;
                _modelFetcher = new RemoteModelFetcher();
            }
        }

        public async Task<IDictionary<string, string>> ProcessAsync(string dtmi, bool includeDepencies = true)
        {
            return await this.ProcessAsync(new List<string>() { dtmi }, includeDepencies);
        }

        public async Task<IDictionary<string, string>> ProcessAsync(IEnumerable<string> dtmis, bool includeDependencies = true)
        {
            Dictionary<string, string> processedModels = new Dictionary<string, string>();
            Queue<string> toProcessModels = new Queue<string>();

            foreach (string dtmi in dtmis)
            {
                toProcessModels.Enqueue(dtmi);
            }

            while (toProcessModels.Count != 0)
            {
                string targetDtmi = toProcessModels.Dequeue();
                if (processedModels.ContainsKey(targetDtmi))
                {
                    _logger.LogInformation($"Already processed DTMI {targetDtmi}. Skipping...");
                    continue;
                }
                _logger.LogInformation($"Processing DTMI '{targetDtmi}'");

                string definition = await this.FetchAsync(targetDtmi);

                if (includeDependencies)
                {
                    List<string> dependencies = this._modelQuery.GetDependencies(definition);
                    if (dependencies.Count > 0)
                        _logger.LogInformation($"Discovered dependencies {string.Join(", ", dependencies)}");

                    foreach (string dep in dependencies)
                    {
                        toProcessModels.Enqueue(dep);
                    }
                }

                if (definition.Contains(targetDtmi, StringComparison.InvariantCulture)) 
                { 
                    processedModels.Add(targetDtmi, definition);
                }
                else
                {
                    // TODO: Msg to include expected -> retrieved. Do after ModelQuery updates.
                    string formatErrorMsg = $"Retrieved model content has incorrect DTMI casing.";
                    throw new ResolverException(targetDtmi, formatErrorMsg, new FormatException(formatErrorMsg));
                }
            }

            return processedModels;
        }

        private async Task<string> FetchAsync(string dtmi)
        {
            try
            {
                return await this._modelFetcher.FetchAsync(dtmi, this.RegistryUri, this._logger);
            }
            catch (Exception ex)
            {
                string fetchErrorMsg = $"Failed retrieving content from '{this.RegistryUri.AbsoluteUri}'.";
                throw new ResolverException(dtmi, fetchErrorMsg, ex);
            }
        }
    }
}
