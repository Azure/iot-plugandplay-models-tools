using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Azure.Iot.ModelsRepository.CLI
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
        }

        public static async Task<int> Export(string dtmi, FileInfo modelFile, string repo, DependencyResolutionOption deps, string output)
        {
            //check that we have either model file or dtmi
            if (string.IsNullOrWhiteSpace(dtmi) && modelFile == null)
            {
                string invalidArgMsg = "Please specify a value for --dtmi";
                Outputs.WriteError(invalidArgMsg);
                return ReturnCodes.InvalidArguments;
            }

            Parsing parsing = new Parsing(repo);
            try
            {
                if (string.IsNullOrWhiteSpace(dtmi))
                {
                    dtmi = parsing.GetRootId(modelFile);
                    if (string.IsNullOrWhiteSpace(dtmi))
                    {
                        Outputs.WriteError("Model is missing root @id");
                        return ReturnCodes.ValidationError;
                    }
                }

                IDictionary<string, string> result = await parsing.GetResolver(resolutionOption: deps).ResolveAsync(dtmi);
                List<string> resultList = result.Values.ToList();
                string normalizedList = string.Join(',', resultList);
                string payload = $"[{normalizedList}]";

                using JsonDocument document = JsonDocument.Parse(payload, CommonOptions.DefaultJsonParseOptions);
                using MemoryStream stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, document.RootElement, CommonOptions.DefaultJsonSerializerOptions);
                stream.Position = 0;
                using StreamReader streamReader = new StreamReader(stream);
                string jsonSerialized = await streamReader.ReadToEndAsync();

                Outputs.WriteOut(jsonSerialized);

                if (!string.IsNullOrEmpty(output))
                {
                    UTF8Encoding utf8WithoutBom = new UTF8Encoding(false);
                    await File.WriteAllTextAsync(output, jsonSerialized, utf8WithoutBom);
                }
            }
            catch (ResolverException resolverEx)
            {
                Outputs.WriteError(resolverEx.Message);
                return ReturnCodes.ResolutionError;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                Outputs.WriteError($"Parsing json-ld content. Details: {jsonEx.Message}");
                return ReturnCodes.InvalidArguments;
            }

            return ReturnCodes.Success;
        }

        public static async Task<int> Validate(FileInfo modelFile, string repo, DependencyResolutionOption deps, bool strict)
        {
            Parsing parsing = new Parsing(repo);

            try
            {
                ModelParser parser = parsing.GetParser(resolutionOption: deps);
                FileExtractResult extractResult = parsing.ExtractModels(modelFile);
                List<string> models = extractResult.Models;

                if (models.Count == 0)
                {
                    Outputs.WriteError("No models to validate.");
                    return ReturnCodes.ValidationError;
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

                    string id = parsing.GetRootId(models[0]);
                    Outputs.WriteOut($"- Ensuring DTMIs namespace conformance for model \"{id}\"...");
                    List<string> invalidSubDtmis = Validations.EnsureSubDtmiNamespace(models[0]);
                    if (invalidSubDtmis.Count > 0)
                    {
                        Outputs.WriteError(
                            $"The following DTMI's do not start with the root DTMI namespace:{Environment.NewLine}{string.Join($",{Environment.NewLine}", invalidSubDtmis)}");
                        return ReturnCodes.ValidationError;
                    }

                    // TODO: Evaluate changing how file path validation is invoked.
                    if (Validations.IsRemoteEndpoint(repo))
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
            catch (ResolverException resolverEx)
            {
                Outputs.WriteError(resolverEx.Message);
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

        public static async Task<int> Import(FileInfo modelFile, DirectoryInfo localRepo, DependencyResolutionOption deps, bool strict)
        {
            if (localRepo == null)
            {
                localRepo = new DirectoryInfo(Path.GetFullPath("."));
            }

            Parsing parsing = new Parsing(localRepo.FullName);

            try
            {
                ModelParser parser = parsing.GetParser(resolutionOption: deps);
                FileExtractResult extractResult = parsing.ExtractModels(modelFile);
                List<string> models = extractResult.Models;

                if (models.Count == 0)
                {
                    Outputs.WriteError("No models to import.");
                    return ReturnCodes.ValidationError;
                }

                Outputs.WriteOut($"- Validating models conform to DTDL...");
                await parser.ParseAsync(models);

                if (strict)
                {
                    foreach (string content in models)
                    {
                        string id = parsing.GetRootId(content);
                        Outputs.WriteOut($"- Ensuring DTMIs namespace conformance for model \"{id}\"...");
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
                    await ModelImporter.ImportAsync(content, localRepo);
                }
            }
            catch (ResolutionException resolutionEx)
            {
                Outputs.WriteError(resolutionEx.Message);
                return ReturnCodes.ResolutionError;
            }
            catch (ResolverException resolverEx)
            {
                Outputs.WriteError(resolverEx.Message);
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
    }
}
