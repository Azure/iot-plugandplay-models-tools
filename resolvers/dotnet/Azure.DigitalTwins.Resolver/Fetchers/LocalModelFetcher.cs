using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;

namespace Azure.DigitalTwins.Resolver.Fetchers
{
    public class LocalModelFetcher : IModelFetcher
    {
        public async Task<string> FetchAsync(string dtmi, Uri registryUri)
        {
            string registryPath = registryUri.AbsolutePath;

            if (!Directory.Exists(registryPath))
            {
                throw new DirectoryNotFoundException($@"Local registry directory '{registryPath}' not found or not accessible.");
            }

            string dtmiFilePath = Utility.DtmiToFilePath(dtmi, registryPath);

            if (!File.Exists(dtmiFilePath))
            {
                throw new FileNotFoundException($@"Model file '{dtmiFilePath}' not found or not accessible in local registry directory '{registryPath}'");
            }

            return await File.ReadAllTextAsync(dtmiFilePath, Encoding.UTF8);
        }
    }
}
