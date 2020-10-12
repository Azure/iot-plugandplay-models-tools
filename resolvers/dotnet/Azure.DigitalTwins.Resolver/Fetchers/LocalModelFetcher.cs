using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Azure.DigitalTwins.Resolver.Fetchers
{
    public class LocalModelFetcher : IModelFetcher
    {
        private readonly ILogger _logger;

        public LocalModelFetcher(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<FetchResult> FetchAsync(string dtmi, Uri registryUri, bool expanded = false)
        {
            string registryPath = registryUri.AbsolutePath;

            if (!Directory.Exists(registryPath))
            {
                string dnfError = StandardStrings.ErrorAccessLocalRepository(registryPath);
                _logger.LogError(dnfError);
                throw new DirectoryNotFoundException(dnfError);
            }

            Queue<string> work = new Queue<string>();

            if (expanded)
                work.Enqueue(GetPath(dtmi, registryUri, true));

            work.Enqueue(GetPath(dtmi, registryUri, false));

            string fnfError = string.Empty;
            while (work.Count != 0)
            {
                string tryContentPath = work.Dequeue();
                _logger.LogInformation(StandardStrings.FetchingContent(tryContentPath));

                if (EvaluatePath(tryContentPath))
                {
                    return new FetchResult()
                    {
                        Definition = await File.ReadAllTextAsync(tryContentPath, Encoding.UTF8),
                        Path = tryContentPath
                    };
                }

                fnfError = StandardStrings.ErrorAccessLocalRepositoryModel(tryContentPath);
                _logger.LogWarning(fnfError);
            }

            throw new FileNotFoundException(fnfError);
        }

        public string GetPath(string dtmi, Uri registryUri, bool expanded = false)
        {
            string registryPath = registryUri.AbsolutePath;
            return DtmiConventions.ToPath(dtmi, registryPath, expanded);
        }

        private bool EvaluatePath(string path)
        {
            return File.Exists(path);
        }
    }
}
