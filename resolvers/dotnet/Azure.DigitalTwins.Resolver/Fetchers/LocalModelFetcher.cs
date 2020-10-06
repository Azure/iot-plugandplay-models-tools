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
                string dnfError = StdStrings.ErrorAccessLocalRepository(registryPath);
                logger.LogError(dnfError);
                throw new DirectoryNotFoundException(dnfError);
            }

            string dtmiFilePath = GetPath(dtmi, registryUri);
            logger.LogInformation(StdStrings.FetchingContent(dtmiFilePath));

            if (!File.Exists(dtmiFilePath))
            {
                string fnfError = StdStrings.ErrorAccessLocalRepositoryModel(dtmiFilePath);
                logger.LogError(fnfError);
                throw new FileNotFoundException(fnfError);
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
