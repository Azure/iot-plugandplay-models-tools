using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;

namespace Azure.IoT.DeviceModelsRepository.Resolver.Fetchers
{
    public class LocalModelFetcher : IModelFetcher
    {
        private readonly ILogger _logger;
        private readonly bool _tryExpanded;

        public LocalModelFetcher(ILogger logger, ResolverClientOptions clientOptions)
        {
            _logger = logger;
            _tryExpanded = clientOptions.DependencyResolution == DependencyResolutionOption.TryFromExpanded;
        }

        public async Task<FetchResult> FetchAsync(string dtmi, Uri repositoryUri, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Fetch(dtmi, repositoryUri, cancellationToken));
        }

        public FetchResult Fetch(string dtmi, Uri repositoryUri, CancellationToken cancellationToken = default)
        {
            string registryPath = repositoryUri.AbsolutePath;

            if (!Directory.Exists(registryPath))
            {
                string dnfError = StandardStrings.ErrorAccessLocalRepository(registryPath);
                _logger.LogError(dnfError);
                throw new DirectoryNotFoundException(dnfError);
            }

            Queue<string> work = new Queue<string>();

            if (_tryExpanded)
                work.Enqueue(GetPath(dtmi, repositoryUri, true));

            work.Enqueue(GetPath(dtmi, repositoryUri, false));

            string fnfError = string.Empty;
            while (work.Count != 0 && !cancellationToken.IsCancellationRequested)
            {
                string tryContentPath = work.Dequeue();
                _logger.LogTrace(StandardStrings.FetchingContent(tryContentPath));

                if (EvaluatePath(tryContentPath))
                {
                    return new FetchResult()
                    {
                        Definition = File.ReadAllText(tryContentPath, Encoding.UTF8),
                        Path = tryContentPath
                    };
                }

                fnfError = StandardStrings.ErrorAccessLocalRepositoryModel(tryContentPath);
                _logger.LogWarning(fnfError);
            }

            throw new FileNotFoundException(fnfError);
        }

        public string GetPath(string dtmi, Uri repositoryUri, bool expanded = false)
        {
            string registryPath = repositoryUri.AbsolutePath;
            return DtmiConventions.DtmiToQualifiedPath(dtmi, registryPath, expanded);
        }

        private bool EvaluatePath(string path)
        {
            return File.Exists(path);
        }
    }
}
