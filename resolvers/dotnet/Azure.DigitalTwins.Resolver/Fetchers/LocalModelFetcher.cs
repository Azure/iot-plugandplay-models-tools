using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Azure.DigitalTwins.Resolver.Fetchers
{
    public class LocalModelFetcher : IModelFetcher
    {
        public async Task<string> FetchAsync(string dtmi, Uri registryUri, ILogger logger)
        {
            string registryPath = registryUri.AbsolutePath;

            if (!Directory.Exists(registryPath))
            {
                string dnfError = $"Local registry directory '{registryPath}' not found or not accessible.";
                logger.LogError(dnfError);
                throw new DirectoryNotFoundException($"Local registry directory '{registryPath}' not found or not accessible.");
            }

            string dtmiFilePath = GetPath(dtmi, registryUri);
            logger.LogInformation($"Attempting to retrieve model content from {dtmiFilePath}");

            if (!File.Exists(dtmiFilePath))
            {
                string fnfError = $"Model file '{dtmiFilePath}' not found or not accessible in local registry directory '{registryPath}'";
                logger.LogError(fnfError);
                throw new FileNotFoundException($"Model file '{dtmiFilePath}' not found or not accessible in local registry directory '{registryPath}'");
            }

            return await File.ReadAllTextAsync(dtmiFilePath, Encoding.UTF8);
        }

        public string GetPath(string dtmi, Uri registryUri)
        {
            string registryPath = registryUri.AbsolutePath;
            return DtmiConventions.ToPath(dtmi, registryPath);
        }
    }
}
