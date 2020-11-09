using Azure.IoT.DeviceModelsRepository.Resolver;
using Azure.IoT.DeviceModelsRepository.Resolver.Extensions;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    internal class Parsing
    {
        private readonly string _repository;

        public Parsing(string repository)
        {
            _repository = repository;
        }

        public ModelParser GetParser(DependencyResolutionOption resolutionOption = DependencyResolutionOption.Enabled)
        {
            ResolverClient client = GetResolver(resolutionOption);
            ModelParser parser = new ModelParser
            {
                DtmiResolver = client.ParserDtmiResolver
            };
            return parser;
        }

        public ResolverClient GetResolver(DependencyResolutionOption resolutionOption = DependencyResolutionOption.Enabled)
        {
            string repository = _repository;
            if (Validations.IsRelativePath(repository))
            {
                repository = Path.GetFullPath(repository);
            }

            return new ResolverClient(
                repository,
                new ResolverClientOptions(resolutionOption));
        }

        public FileExtractResult ExtractModels(FileInfo modelsFile)
        {
            string modelsText = File.ReadAllText(modelsFile.FullName);
            return ExtractModels(modelsText);
        }

        public FileExtractResult ExtractModels(string modelsText)
        {
            List<string> result = new List<string>();
            using JsonDocument document = JsonDocument.Parse(modelsText);
            JsonElement root = document.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                result.Add(root.GetRawText());
                return new FileExtractResult(result, root.ValueKind);
            }
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    result.Add(element.GetRawText());
                }
                return new FileExtractResult(result, root.ValueKind);
            }

            throw new ArgumentException($"Importing model file contents of kind {root.ValueKind} is not yet supported.");
        }

        public string GetRootId(string modelText)
        {
            using JsonDocument document = JsonDocument.Parse(modelText);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("@id", out JsonElement id))
            {
                if (id.ValueKind == JsonValueKind.String)
                {
                    return id.GetString();
                }
            }

            return string.Empty;
        }

        public string GetRootId(FileInfo fileInfo)
        {
            return GetRootId(File.ReadAllText(fileInfo.FullName));
        }
    }
}
