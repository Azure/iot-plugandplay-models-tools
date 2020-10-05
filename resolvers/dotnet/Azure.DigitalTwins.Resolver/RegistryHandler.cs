using Azure.DigitalTwins.Resolver.Fetchers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver
{
    public class RegistryHandler
    {
        private readonly IModelFetcher _modelFetcher;
        private readonly ILogger _logger;

        public enum RegistryTypeCategory
        {
            RemoteUri,
            LocalUri
        }

        public Uri RegistryUri { get; }
        public RegistryTypeCategory RegistryType { get; }

        public RegistryHandler(Uri registryUri, ILogger logger = null)
        {
            _logger = logger ?? NullLogger.Instance;
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

        public string ToPath(string dtmi)
        {
            return _modelFetcher.GetPath(dtmi, this.RegistryUri);
        }

        public static bool IsValidDtmi(string dtmi)
        {
            // Regex defined at https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
            Regex rx = new Regex(@"^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
            return rx.IsMatch(dtmi);
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
                if (!IsValidDtmi(dtmi))
                {
                    string invalidArgMsg = $"Input DTMI '{dtmi}' has an invalid format.";
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
                    _logger.LogInformation($"Already processed DTMI {targetDtmi}. Skipping...");
                    continue;
                }
                _logger.LogInformation($"Processing DTMI '{targetDtmi}'");

                string definition = await this.FetchAsync(targetDtmi);
                ModelQuery.ModelMetadata metadata = new ModelQuery(definition).GetMetadata();

                if (includeDependencies)
                {
                    IList<string> dependencies = metadata.Dependencies;

                    if (dependencies.Count > 0)
                        _logger.LogInformation($"Discovered dependencies {string.Join(", ", dependencies)}");

                    foreach (string dep in dependencies)
                    {
                        toProcessModels.Enqueue(dep);
                    }
                }

                string parsedDtmi = metadata.Id;
                if (!parsedDtmi.Equals(targetDtmi, StringComparison.Ordinal))
                {
                    string formatErrorMsg =
                        $"Retrieved model content has incorrect DTMI casing. Expected {targetDtmi}, parsed {parsedDtmi}";
                    throw new ResolverException(targetDtmi, formatErrorMsg, new FormatException(formatErrorMsg));
                }

                processedModels.Add(targetDtmi, definition);
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
