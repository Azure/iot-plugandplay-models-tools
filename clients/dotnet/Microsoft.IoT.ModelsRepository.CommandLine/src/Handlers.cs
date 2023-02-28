// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure;
using Azure.IoT.ModelsRepository;
using DTDLParser;
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
                Outputs.WriteError($"Failure parsing json-ld content. Details: {jsonEx.Message}");
                return ReturnCodes.InvalidArguments;
            }

            return ReturnCodes.Success;
        }

        public static async Task<int> Validate(FileInfo modelFile, DirectoryInfo directory, string searchPattern, string repo, bool strict, int maxDtdlVersion)
        {
            try
            {
                ValidationRules validationRules = strict ? new ValidationRules() : ValidationRules.GetJustParseRules();
                
                RepoProvider repoProvider = new RepoProvider(repo);
                if (modelFile != null && modelFile.Exists)
                {
                    return await Validations.ValidateModelFileAsync(modelFile, repoProvider, validationRules, maxDtdlVersion);
                }

                if (directory != null && directory.Exists)
                {
                    int result;
                    foreach (string file in Directory.EnumerateFiles(directory.FullName, searchPattern,
                        new EnumerationOptions { RecurseSubdirectories = true }))
                    {
                        var enumeratedFile = new FileInfo(file);
                        result = await Validations.ValidateModelFileAsync(enumeratedFile, repoProvider, validationRules, maxDtdlVersion);

                        // TODO: Consider processing modes "return on first error", "return all errors"
                        if (result != ReturnCodes.Success)
                        {
                            return result;
                        }
                    }

                    return ReturnCodes.Success;
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

            Outputs.WriteError("Nothing to validate!");
            return ReturnCodes.InvalidArguments;
        }

        internal static async IAsyncEnumerable<string> ToAsyncEnumerable(List<string> strings)
        {
            foreach (string s in strings)
            {
                yield return s;
            }
            await Task.Yield();
        }

        public static async Task<int> Import(FileInfo modelFile, DirectoryInfo directory, string searchPattern, DirectoryInfo localRepo, bool force, int maxDtdlVersion)
        {
            if (localRepo == null)
            {
                localRepo = new DirectoryInfo(Path.GetFullPath("."));
            }

            try
            {
                RepoProvider repoProvider = new RepoProvider(localRepo.FullName);
                if (modelFile != null && modelFile.Exists)
                {
                    var importFileValidationRules = new ValidationRules(
                        parseDtdl: true,
                        ensureFilePlacement: false,
                        ensureContentRootType: false,
                        ensureDtmiNamespace: true);

                    return await ModelImporter.ImportFileAsync(modelFile, localRepo, repoProvider, force, importFileValidationRules);
                }

                // When importing models from an arbitrary directory we have to extract all models content
                // and parse all at once because arbitrary model directories do not have consistent model file
                // placement compared to DMR-like repositories when resolving model dependencies.
                if (directory != null && directory.Exists)
                {
                    var contentMap = new Dictionary<FileInfo, List<string>>();
                    foreach (string file in Directory.EnumerateFiles(directory.FullName, searchPattern,
                        new EnumerationOptions { RecurseSubdirectories = true }))
                    {
                        var enumeratedFile = new FileInfo(file);
                        FileExtractResult extractResult = ParsingUtils.ExtractModels(enumeratedFile);
                        List<string> models = extractResult.Models;
                        contentMap.Add(enumeratedFile, models);
                    }

                    var flatModelsContent = new List<string>();
                    foreach(KeyValuePair<FileInfo, List<string>> entry in contentMap)
                    {
                        flatModelsContent.AddRange(entry.Value);
                    }
                    ModelParser parser = repoProvider.GetDtdlParser(maxDtdlVersion);
                    await parser.ParseAsync(ToAsyncEnumerable(flatModelsContent));

                    var importDirectoryValidationRules = new ValidationRules(
                        parseDtdl: false, // All the directory models content is parsed at once earlier.
                        ensureFilePlacement: false,
                        ensureContentRootType: false,
                        ensureDtmiNamespace: true);

                    int result;
                    foreach (KeyValuePair<FileInfo, List<string>> entry in contentMap)
                    {
                        result = await ModelImporter.ImportFileAsync(entry.Key, localRepo, repoProvider, force, importDirectoryValidationRules);
                        if (result != ReturnCodes.Success)
                        {
                            return result;
                        }
                    }

                    return ReturnCodes.Success;
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

            Outputs.WriteError("Nothing to import!");
            return ReturnCodes.InvalidArguments;
        }

        public static int RepoIndex(DirectoryInfo localRepo, FileInfo outputFile, int pageLimit)
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

            int currentPageCount = 0;
            int pageIdentifier = 1;
            FileInfo currentPageFile = outputFile;
            var modelDictionary = new ModelDictionary();
            var currentLinks = new ModelIndexLinks
            {
                Self = currentPageFile.FullName
            };

            foreach (string file in Directory.EnumerateFiles(localRepo.FullName, "*.json",
                new EnumerationOptions { RecurseSubdirectories = true }))
            {
                if (file.ToLower().EndsWith(".expanded.json")){
                    continue;
                }

                // TODO: Debug plumbing.
                // Outputs.WriteDebug($"Processing: {file}");

                try
                {
                    var modelFile = new FileInfo(file);
                    ModelIndexEntry indexEntry = ParsingUtils.ParseModelFileForIndex(modelFile);
                    modelDictionary.Add(indexEntry.Dtmi, indexEntry);
                    currentPageCount += 1;

                    if (currentPageCount == pageLimit)
                    {
                        var nextPageFile = new FileInfo(
                            Path.Combine(
                                currentPageFile.Directory.FullName,
                                $"index.page.{pageIdentifier}.json"));
                        currentLinks.Next = nextPageFile.FullName;
                        var nextLinks = new ModelIndexLinks
                        {
                            Self = nextPageFile.FullName,
                            Prev = currentLinks.Self
                        };

                        var modelIndex = new ModelIndex(modelDictionary, currentLinks);
                        IndexPageUtils.WritePage(modelIndex);
                        currentPageCount -= pageLimit;
                        modelDictionary = new ModelDictionary();
                        currentPageFile = nextPageFile;
                        currentLinks = nextLinks;
                        pageIdentifier += 1;
                    }
                }
                catch(Exception e)
                {
                    Outputs.WriteError($"Failure processing model file: {file}, {e.Message}");
                    return ReturnCodes.ProcessingError;
                }
            }

            if (modelDictionary.Count > 0)
            {
                var modelIndex = new ModelIndex(modelDictionary, currentLinks);
                IndexPageUtils.WritePage(modelIndex);
            }

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
                    Outputs.WriteError($"Failure expanding model file: {file}, {e.Message}");
                    return ReturnCodes.ProcessingError;
                }
            }

            return ReturnCodes.Success;
        }
    }
}
