// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

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
        readonly string _repoLocationUriStr;
        readonly Uri _repoLocationUri;

        public RepoProvider(string repoLocationUriStr)
        {
            _repoLocationUriStr = repoLocationUriStr;
            if (IsRelativePath(_repoLocationUriStr))
            {
                _repoLocationUriStr = Path.GetFullPath(_repoLocationUriStr);
            }

            _repoLocationUri = new Uri(_repoLocationUriStr);
            _repositoryClient = new ModelsRepositoryClient(_repoLocationUri);
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

        // TODO: Convert to instance method.
        public static bool IsRelativePath(string repositoryPath)
        {
            bool validUri = Uri.TryCreate(repositoryPath, UriKind.Relative, out Uri testUri);
            return validUri && testUri != null;
        }

        public bool IsRemoteEndpoint()
        {
            bool validUri = Uri.TryCreate(_repoLocationUriStr, UriKind.Absolute, out Uri testUri);
            return validUri && testUri != null && testUri.Scheme != "file";
        }

        public Uri RepoLocationUri
        {
            get { return _repoLocationUri; }
        }
    }
}
