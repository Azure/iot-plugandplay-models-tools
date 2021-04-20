using Azure.IoT.ModelsRepository;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.IoT.ModelsRepository.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class RepoProvider
    {
        readonly ModelsRepositoryClient _repositoryClient;

        public RepoProvider(string repoLocationUri,
            ModelDependencyResolution dependencyResolution = ModelDependencyResolution.Enabled)
        {
            if (IsRelativePath(repoLocationUri))
            {
                repoLocationUri = Path.GetFullPath(repoLocationUri);
            }

            _repositoryClient = new ModelsRepositoryClient(new Uri(repoLocationUri),
                new ModelsRepositoryClientOptions(dependencyResolution: dependencyResolution));
        }

        public async Task<List<string>> ExpandModel(FileInfo modelFile)
        {
            string dtmi = ParsingUtils.GetRootId(modelFile);
            return await ExpandModel(dtmi);
        }

        public async Task<List<string>> ExpandModel(string dtmi)
        {
            IDictionary<string, string> modelResult = await _repositoryClient.GetModelsAsync(dtmi);
            return ConvertToExpanded(dtmi, modelResult);
        }

        private List<string> ConvertToExpanded(string rootDtmi, IDictionary<string, string> models)
        {
            var result = new List<string>
            {
                models[rootDtmi]
            };
            models.Remove(rootDtmi);
            result.AddRange(models.Values);

            return result;
        }

        public ModelParser GetDtdlParser()
        {
            ModelParser parser = new ModelParser
            {
                DtmiResolver = _repositoryClient.ParserDtmiResolver
            };
            return parser;
        }

        public static bool IsRelativePath(string repositoryPath)
        {
            bool validUri = Uri.TryCreate(repositoryPath, UriKind.Relative, out Uri testUri);
            return validUri && testUri != null;
        }

        public static bool IsRemoteEndpoint(string repositoryPath)
        {
            bool validUri = Uri.TryCreate(repositoryPath, UriKind.Absolute, out Uri testUri);
            return validUri && testUri != null && testUri.Scheme != "file";
        }
    }
}
