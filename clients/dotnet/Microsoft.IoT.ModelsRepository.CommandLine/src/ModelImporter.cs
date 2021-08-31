// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.IoT.ModelsRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class ModelImporter
    {
        public static void Import(string modelContent, DirectoryInfo repository)
        {
            string rootId = ParsingUtils.GetRootId(modelContent);
            string createPath = DtmiConventions.GetModelUri(rootId, new Uri(repository.FullName)).LocalPath;

            Outputs.WriteOut($"* Importing model \"{rootId}\".");
            if (File.Exists(createPath))
            {
                Outputs.WriteOut(
                    $"* Skipping \"{rootId}\". Model file already exists in repository.",
                    ConsoleColor.DarkCyan);
                return;
            }

            (new FileInfo(createPath)).Directory.Create();
            Outputs.WriteToFile(createPath, modelContent);
        }

        public static async Task<int> ImportFileAsync(FileInfo modelFile, DirectoryInfo repository, RepoProvider repoProvider, ValidationRules rules=null)
        {
            int validationResult = await Validations.ValidateModelFileAsync(modelFile, repoProvider, rules);
            if (validationResult != ReturnCodes.Success)
            {
                return validationResult;
            }

            // TODO: Redundant
            FileExtractResult extractResult = ParsingUtils.ExtractModels(modelFile);
            List<string> models = extractResult.Models;

            foreach (string content in models)
            {
                Import(content, repository);
            }

            return ReturnCodes.Success;
        }
    }
}
