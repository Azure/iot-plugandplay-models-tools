// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.IoT.ModelsRepository;
using DTDLParser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class Validations
    {
        internal const int DefaultMaxDtdlVersion = 2;

        public static string EnsureValidModelFilePath(string modelFilePath, string modelContent, string repository)
        {
            if (RepoProvider.IsRelativePath(repository))
            {
                repository = Path.GetFullPath(repository);
            }

            string rootId = ParsingUtils.GetRootId(modelContent);
            Uri targetModelPathUri = DtmiConventions.GetModelUri(rootId, new Uri(repository));
            Uri modelFilePathUri = new Uri(modelFilePath);

            if (targetModelPathUri.AbsolutePath != modelFilePathUri.AbsolutePath)
            {
                return Path.GetFullPath(targetModelPathUri.AbsolutePath);
            }

            return null;
        }

        public static List<string> ScanIdsForReservedWords(string fileText)
        {
            List<string> badIds = new List<string>();
            var reservedRegEx = new Regex("Microsoft|Azure", RegexOptions.IgnoreCase);

            FindAllIds(fileText, (id) =>
            {
                if (reservedRegEx.IsMatch(id))
                    badIds.Add(id);
            });

            return badIds;
        }

        public static List<string> EnsureSubDtmiNamespace(string fileText)
        {
            List<string> badIds = new List<string>();
            string dtmiNamespace = GetDtmiNamespace(ParsingUtils.GetRootId(fileText));

            FindAllIds(fileText, (id) =>
            {
                if (!id.StartsWith(dtmiNamespace))
                    badIds.Add(id);
            });

            return badIds;
        }

        private static void FindAllIds(string fileText, Action<string> validation)
        {
            var idRegex = new Regex("\\\"@id\\\":\\s?\\\"[^\\\"]*\\\",?");
            foreach (Match id in idRegex.Matches(fileText))
            {
                // return just the value without "@id" and quotes
                var idValue = Regex.Replace(Regex.Replace(id.Value, "\\\"@id\\\":\\s?\"", ""), "\",?", "");
                validation(idValue);
            }
        }

        public static string GetDtmiNamespace(string rootId)
        {
            var versionRegex = new Regex(";[1-9][0-9]{0,8}$");
            return versionRegex.Replace(rootId, "");
        }

        public static async Task<int> ValidateModelFileAsync(
            FileInfo modelFile, RepoProvider repoProvider, ValidationRules rules = null, int maxDtdlVersion = DefaultMaxDtdlVersion)
        {
            if (rules == null)
            {
                rules = new ValidationRules();
            }

            Outputs.WriteOut($"[Validating]: {modelFile.FullName}");

            FileExtractResult extractResult = ParsingUtils.ExtractModels(modelFile);
            List<string> models = extractResult.Models;

            if (models.Count == 0)
            {
                Outputs.WriteError("No models to validate.");
                return ReturnCodes.InvalidArguments;
            }

            ModelParser parser;
            if (models.Count >= 1 && extractResult.ContentKind == JsonValueKind.Array)
            {
                // Special case: when validating from an array, only use array contents for resolution.
                // Setup vanilla parser with no resolution. We get a better error message when a delegate is assigned.
                // TODO: rido, review error message from this comment
                parser = new ModelParser();
            }
            else
            {
                parser = repoProvider.GetDtdlParser(maxDtdlVersion);
            }

            // TODO: Extract strings
            Outputs.WriteOut($"* Validating model file content conforms to DTDL.");

            if (rules.ParseDtdl)
            {
                await parser.ParseAsync(Handlers.ToAsyncEnumerable(models));
            }

            if (rules.EnsureContentRootType)
            {
                if (extractResult.ContentKind == JsonValueKind.Array || models.Count > 1)
                {
                    // Related to file path validation.
                    Outputs.WriteError("Strict validation requires a single root model object.");
                    return ReturnCodes.ValidationError;
                }
            }

            if (rules.EnsureDtmiNamespace)
            {
                string dtmi = ParsingUtils.GetRootId(models[0]);
                Outputs.WriteOut($"* Ensuring DTMIs namespace conformance for model \"{dtmi}\".");
                List<string> invalidSubDtmis = EnsureSubDtmiNamespace(models[0]);
                if (invalidSubDtmis.Count > 0)
                {
                    Outputs.WriteError(
                        $"The following DTMI's do not start with the root DTMI namespace:{Environment.NewLine}{string.Join($",{Environment.NewLine}", invalidSubDtmis)}");
                    return ReturnCodes.ValidationError;
                }
            }

            if (rules.EnsureFilePlacement)
            {
                if (repoProvider.IsRemoteEndpoint())
                {
                    Outputs.WriteError($"Model file path validation requires a local repository.");
                    return ReturnCodes.ValidationError;
                }

                Outputs.WriteOut($"* Ensuring model file path adheres to DMR path conventions.");
                string filePathError = EnsureValidModelFilePath(modelFile.FullName, models[0], repoProvider.RepoLocationUri.LocalPath);

                if (filePathError != null)
                {
                    Outputs.WriteError(
                        $"File \"{modelFile.FullName}\" does not adhere to DMR path conventions. Expecting \"{filePathError}\".");
                    return ReturnCodes.ValidationError;
                }
            }

            return ReturnCodes.Success;
        }
    }
}
