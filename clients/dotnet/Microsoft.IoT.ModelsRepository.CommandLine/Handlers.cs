using Azure;
using Azure.IoT.ModelsRepository;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;


namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal static class Handlers
    {
        // Alternative to enum to avoid casting.
        public static class ReturnCodes
        {
            public const int Success = 0;
            public const int InvalidArguments = 1;
            public const int ValidationError = 2;
            public const int ResolutionError = 3;
            public const int ProcessingError = 4;
        }

        public static async Task<int> Export(string dtmi, FileInfo modelFile, string repo, FileInfo outputFile)
        {
            // Check that we have either model file or dtmi
            if (string.IsNullOrWhiteSpace(dtmi) && modelFile == null)
            {
                string invalidArgMsg = "Please specify a value for --dtmi";
                Outputs.WriteError(invalidArgMsg);
                return ReturnCodes.InvalidArguments;
            }

            var repoProvider = new RepoProvider(repo);

            try
            {
                if (string.IsNullOrWhiteSpace(dtmi))
                {
                    dtmi = ParsingUtils.GetRootId(modelFile);
                    if (string.IsNullOrWhiteSpace(dtmi))
                    {
                        Outputs.WriteError("Model is missing root @id");
                        return ReturnCodes.ValidationError;
                    }
                }

                List<string> expandedModel = await repoProvider.ExpandModel(dtmi);
                string formattedJson = Outputs.FormatExpandedListAsJson(expandedModel);

                Outputs.WriteOut(formattedJson);
                Outputs.WriteToFile(outputFile, formattedJson);
            }
            catch (RequestFailedException requestEx)
            {
                Outputs.WriteError(requestEx.Message);
                return ReturnCodes.ResolutionError;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                Outputs.WriteError($"Parsing json-ld content. Details: {jsonEx.Message}");
                return ReturnCodes.InvalidArguments;
            }

            return ReturnCodes.Success;
        }

        public static async Task<int> Validate(FileInfo modelFile, string repo, bool strict)
        {
            try
            {
                RepoProvider repoProvider = new RepoProvider(repo);
                FileExtractResult extractResult = ParsingUtils.ExtractModels(modelFile);
                List<string> models = extractResult.Models;

                if (models.Count == 0)
                {
                    Outputs.WriteError("No models to validate.");
                    return ReturnCodes.InvalidArguments;
                }

                ModelParser parser;
                if (models.Count > 1)
                {
                    // Special case: when validating from an array, only use array contents for resolution.
                    // Setup vanilla parser with no resolution. We get a better error message when a delegate is assigned.
                    parser = new ModelParser
                    {
                        DtmiResolver = (IReadOnlyCollection<Dtmi> dtmis) =>
                        {
                            return Task.FromResult(Enumerable.Empty<string>());
                        }
                    };
                }
                else
                {
                    parser = repoProvider.GetDtdlParser();
                }

                Outputs.WriteOut($"- Validating models conform to DTDL...");
                await parser.ParseAsync(models);

                if (strict)
                {
                    if (extractResult.ContentKind == JsonValueKind.Array || models.Count > 1)
                    {
                        // Related to file path validation.
                        Outputs.WriteError("Strict validation requires a single root model object.");
                        return ReturnCodes.ValidationError;
                    }

                    string dtmi = ParsingUtils.GetRootId(models[0]);
                    Outputs.WriteOut($"- Ensuring DTMIs namespace conformance for model \"{dtmi}\"...");
                    List<string> invalidSubDtmis = Validations.EnsureSubDtmiNamespace(models[0]);
                    if (invalidSubDtmis.Count > 0)
                    {
                        Outputs.WriteError(
                            $"The following DTMI's do not start with the root DTMI namespace:{Environment.NewLine}{string.Join($",{Environment.NewLine}", invalidSubDtmis)}");
                        return ReturnCodes.ValidationError;
                    }

                    // TODO: Evaluate changing how file path validation is invoked.
                    if (RepoProvider.IsRemoteEndpoint(repo))
                    {
                        Outputs.WriteError($"Model file path validation requires a local repository.");
                        return ReturnCodes.ValidationError;
                    }

                    Outputs.WriteOut($"- Ensuring model file path adheres to DMR path conventions...");
                    string filePathError = Validations.EnsureValidModelFilePath(modelFile.FullName, models[0], repo);

                    if (filePathError != null)
                    {
                        Outputs.WriteError(
                            $"File \"{modelFile.FullName}\" does not adhere to DMR path conventions. Expecting \"{filePathError}\".");
                        return ReturnCodes.ValidationError;
                    }
                }
            }
            catch (ResolutionException resolutionEx)
            {
                Outputs.WriteError(resolutionEx.Message);
                return ReturnCodes.ResolutionError;
            }
            catch (RequestFailedException requestEx)
            {
                Outputs.WriteError(requestEx.Message);
                return ReturnCodes.ResolutionError;
            }
            catch (ParsingException parsingEx)
            {
                IList<ParsingError> errors = parsingEx.Errors;
                string normalizedErrors = string.Empty;
                foreach (ParsingError error in errors)
                {
                    normalizedErrors += $"{Environment.NewLine}{error.Message}";
                }

                Outputs.WriteError(normalizedErrors);
                return ReturnCodes.ValidationError;
            }
            catch (IOException ioEx)
            {
                Outputs.WriteError(ioEx.Message);
                return ReturnCodes.InvalidArguments;
            }
            catch (ArgumentException argEx)
            {
                Outputs.WriteError(argEx.Message);
                return ReturnCodes.InvalidArguments;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                Outputs.WriteError($"Parsing json-ld content. Details: {jsonEx.Message}");
                return ReturnCodes.InvalidArguments;
            }

            return ReturnCodes.Success;
        }

        public static async Task<int> Import(FileInfo modelFile, DirectoryInfo localRepo, bool strict)
        {
            if (localRepo == null)
            {
                localRepo = new DirectoryInfo(Path.GetFullPath("."));
            }

            try
            {
                RepoProvider repoProvider = new RepoProvider(localRepo.FullName);
                FileExtractResult extractResult = ParsingUtils.ExtractModels(modelFile);
                List<string> models = extractResult.Models;

                if (models.Count == 0)
                {
                    Outputs.WriteError("No models to import.");
                    return ReturnCodes.ValidationError;
                }

                Outputs.WriteOut($"- Validating models conform to DTDL...");
                ModelParser parser = repoProvider.GetDtdlParser();
                await parser.ParseAsync(models);

                if (strict)
                {
                    foreach (string content in models)
                    {
                        string dtmi = ParsingUtils.GetRootId(content);
                        Outputs.WriteOut($"- Ensuring DTMIs namespace conformance for model \"{dtmi}\"...");
                        List<string> invalidSubDtmis = Validations.EnsureSubDtmiNamespace(content);
                        if (invalidSubDtmis.Count > 0)
                        {
                            Outputs.WriteError(
                                $"The following DTMI's do not start with the root DTMI namespace:{Environment.NewLine}{string.Join($",{Environment.NewLine}", invalidSubDtmis)}");
                            return ReturnCodes.ValidationError;
                        }
                    }
                }

                foreach (string content in models)
                {
                    ModelImporter.Import(content, localRepo);
                }
            }
            catch (ResolutionException resolutionEx)
            {
                Outputs.WriteError(resolutionEx.Message);
                return ReturnCodes.ResolutionError;
            }
            catch (RequestFailedException requestEx)
            {
                Outputs.WriteError(requestEx.Message);
                return ReturnCodes.ResolutionError;
            }
            catch (ParsingException parsingEx)
            {
                IList<ParsingError> errors = parsingEx.Errors;
                string normalizedErrors = string.Empty;
                foreach (ParsingError error in errors)
                {
                    normalizedErrors += $"{Environment.NewLine}{error.Message}";
                }

                Outputs.WriteError(normalizedErrors);
                return ReturnCodes.ValidationError;
            }
            catch (IOException ioEx)
            {
                Outputs.WriteError(ioEx.Message);
                return ReturnCodes.InvalidArguments;
            }
            catch (ArgumentException argEx)
            {
                Outputs.WriteError(argEx.Message);
                return ReturnCodes.InvalidArguments;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                Outputs.WriteError($"Parsing json-ld content. Details: {jsonEx.Message}");
                return ReturnCodes.InvalidArguments;
            }

            return ReturnCodes.Success;
        }

        public static int RepoIndex(DirectoryInfo localRepo, FileInfo outputFile)
        {
            if (localRepo == null)
            {
                localRepo = new DirectoryInfo(Path.GetFullPath("."));
            }

            if (!localRepo.Exists)
            {
                Outputs.WriteError($"Invalid target repository directory: {localRepo.FullName}.");
                return ReturnCodes.InvalidArguments;
            }

            var modelIndex = new ModelIndex();

            foreach (string file in Directory.EnumerateFiles(localRepo.FullName, "*.json",
                new EnumerationOptions { RecurseSubdirectories = true }))
            {
                if (file.ToLower().EndsWith(".expanded.json")){
                    continue;
                }

                Outputs.WriteOut($"Processing: {file}");

                try
                {
                    var modelFile = new FileInfo(file);
                    ModelIndexEntry indexEntry = ParsingUtils.ParseModelFileForIndex(modelFile);
                    modelIndex.Add(indexEntry.Dtmi, indexEntry);
                }
                catch(Exception e)
                {
                    Outputs.WriteError($"Failure processing model file: {file}, {e.Message}");
                    return ReturnCodes.ProcessingError;
                }
            }

            string indexJsonString = JsonSerializer.Serialize(modelIndex, ParsingUtils.DefaultJsonSerializerOptions);
            Outputs.WriteToFile(outputFile, indexJsonString);
            Outputs.WriteOut(indexJsonString);

            return ReturnCodes.Success;
        }

        public static async Task<int> RepoExpand(DirectoryInfo localRepo)
        {
            if (localRepo == null)
            {
                localRepo = new DirectoryInfo(Path.GetFullPath("."));
            }

            var repoProvider = new RepoProvider(localRepo.FullName);

            if (!localRepo.Exists)
            {
                Outputs.WriteError($"Invalid target repository directory: {localRepo.FullName}.");
                return ReturnCodes.InvalidArguments;
            }

            foreach (string file in Directory.EnumerateFiles(localRepo.FullName, "*.json",
                new EnumerationOptions { RecurseSubdirectories = true }))
            {
                if (file.ToLower().EndsWith(".expanded.json"))
                {
                    continue;
                }

                try
                {
                    var modelFile = new FileInfo(file);
                    string dtmi = ParsingUtils.GetRootId(modelFile);

                    if (string.IsNullOrEmpty(dtmi))
                    {
                        continue;
                    }
                    List<string> expandedModel = await repoProvider.ExpandModel(dtmi);
                    string formattedJson = Outputs.FormatExpandedListAsJson(expandedModel);

                    string createPath = DtmiConventions.GetModelUri(dtmi, new Uri(localRepo.FullName), true).AbsolutePath;
                    Outputs.WriteToFile(createPath, formattedJson);
                    Outputs.WriteOut($"Created: {createPath}");
                }
                catch(Exception e)
                {
                    Outputs.WriteError($"Failure processing model file: {file}, {e.Message}");
                    return ReturnCodes.ProcessingError;
                }
            }

            return ReturnCodes.Success;
        }
    }
}
