using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class ParsingUtils
    {
        public static FileExtractResult ExtractModels(FileInfo modelsFile)
        {
            string modelsText = File.ReadAllText(modelsFile.FullName);
            return ExtractModels(modelsText);
        }

        public static FileExtractResult ExtractModels(string modelsText)
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

        public static string GetRootId(FileInfo fileInfo)
        {
            return GetRootId(File.ReadAllText(fileInfo.FullName));
        }

        public static string GetRootId(string modelText)
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

        public static ModelIndexEntry ParseModelFileForIndex(FileInfo fileInfo)
        {
            string modelText = File.ReadAllText(fileInfo.FullName);

            using JsonDocument document = JsonDocument.Parse(modelText);
            JsonElement root = document.RootElement;

            object description = null;
            object displayName = null;

            JsonSerializerOptions options = DefaultJsonSerializerOptions;

            if (root.TryGetProperty("description", out JsonElement descriptionElement))
            {
                description = JsonSerializer.Deserialize<object>(descriptionElement.GetRawText(), options);
            }

            if (root.TryGetProperty("displayName", out JsonElement displaneNameElement))
            {
                displayName = JsonSerializer.Deserialize<object>(displaneNameElement.GetRawText(), options);
            }

            var indexEntry = new ModelIndexEntry()
            {
                Dtmi = root.GetProperty("@id").GetString(),
                Description = description,
                DisplayName = displayName
            };

            return indexEntry;
        }

        public static JsonSerializerOptions DefaultJsonSerializerOptions
        {
            get
            {
                return new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IgnoreNullValues = true,
                    AllowTrailingCommas = true,
                    // DTDL supports these characters.
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
            }
        }

        public static JsonDocumentOptions DefaultJsonParseOptions
        {
            get
            {
                return new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                };
            }
        }
    }
}
