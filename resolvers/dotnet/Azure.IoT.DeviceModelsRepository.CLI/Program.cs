using Azure.IoT.DeviceModelsRepository.CLI.Exceptions;
using Azure.IoT.DeviceModelsRepository.Resolver;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    class Program
    {
        private static readonly string _parserVersion = typeof(ModelParser).Assembly.GetName().Version.ToString();
        private static readonly string _resolverVersion = typeof(ResolverClient).Assembly.GetName().Version.ToString();
        private static readonly string _cliVersion = typeof(Program).Assembly.GetName().Version.ToString();

        // Alternative to enum to avoid casting.
        public static class ReturnCodes
        {
            public const int Success = 0;
            public const int ResolutionError = 1;
            public const int ParserError = 2;
            public const int InvalidArguments = 3;
            public const int ValidationError = 4;
            public const int ImportError = 5;
        }

        static async Task<int> Main(string[] args) => await BuildCommandLine()
            .UseHost(_ => Host.CreateDefaultBuilder())
            .UseDefaults()
            .Build()
            .InvokeAsync(args);

        private static CommandLineBuilder BuildCommandLine()
        {
            RootCommand root = new RootCommand("parent")
            {
                Description = "Microsoft IoT Plug and Play Device Model Repository CLI"
            };

            root.Add(BuildExportCommand());
            root.Add(BuildValidateCommand());
            root.Add(BuildImportModelCommand());

            return new CommandLineBuilder(root);
        }

        private static ILogger GetLogger(IHost host)
        {
            IServiceProvider serviceProvider = host.Services;
            ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            return loggerFactory.CreateLogger(typeof(Program));
        }

        private static void OutputHeaders(ILogger logger)
        {
            logger.LogInformation($"dmr-client/{_cliVersion} parser/{_parserVersion} resolver/{_resolverVersion}");
        }


        private static Command BuildExportCommand()
        {
            Command resolveModel = new Command("export")
            {
                CommonOptions.Dtmi,
                CommonOptions.Repo,
                CommonOptions.Output,
                CommonOptions.Silent,
                CommonOptions.ModelFile
            };

            resolveModel.Description = "Retrieve a model and its dependencies by dtmi or model file using the target repository for model resolution.";
            resolveModel.Handler = CommandHandler.Create<string, string, IHost, string, bool, FileInfo>(async (dtmi, repository, host, output, silent, modelFile) =>
            {
                ILogger logger = GetLogger(host);
                OutputHeaders(logger);

                IDictionary<string, string> result;

                //check that we have either model file or dtmi
                if (string.IsNullOrWhiteSpace(dtmi) && modelFile == null)
                {
                    logger.LogError("Either --dtmi or --model-file must be specified!");
                    return ReturnCodes.InvalidArguments;
                }

                Parsing parsing = new Parsing(repository, logger);
                try
                {
                    if (string.IsNullOrWhiteSpace(dtmi))
                    {
                        dtmi = parsing.GetRootDtmiFromFile(modelFile);
                    }
                    
                    result = await parsing.GetResolver().ResolveAsync(dtmi);
                }
                catch (ResolverException resolverEx)
                {
                    logger.LogError(resolverEx.Message);
                    return ReturnCodes.ResolutionError;
                }
                catch (KeyNotFoundException keyNotFoundEx)
                {
                    logger.LogError(keyNotFoundEx.Message);
                    return ReturnCodes.ParserError;
                }

                List<string> resultList = result.Values.ToList();
                string normalizedList = string.Join(',', resultList);
                string payload = "[" + string.Join(',', normalizedList) + "]";

                using JsonDocument document = JsonDocument.Parse(payload, CommonOptions.DefaultJsonParseOptions);
                using MemoryStream stream = new MemoryStream();
                await JsonSerializer.SerializeAsync(stream, document.RootElement, CommonOptions.DefaultJsonSerializerOptions);
                stream.Position = 0;
                using StreamReader streamReader = new StreamReader(stream);
                string jsonSerialized = await streamReader.ReadToEndAsync();

                if (!silent)
                    await Console.Out.WriteLineAsync(jsonSerialized);

                if (!string.IsNullOrEmpty(output))
                {
                    logger.LogInformation($"Writing result to file '{output}'");
                    await File.WriteAllTextAsync(output, jsonSerialized, Encoding.UTF8);
                }

                return ReturnCodes.Success;
            });

            return resolveModel;
        }

        private static Command BuildValidateCommand()
        {
            var modelFileOption = CommonOptions.ModelFile;
            modelFileOption.IsRequired = true; // Option is required for this command

            Command validateModel = new Command("validate")
            {
                modelFileOption,
                CommonOptions.Repo,
                CommonOptions.Strict
            };

            validateModel.Description = "Validates a model using the DTDL model parser & resolver. " +
                "Uses the target repository for model resolution. ";
            validateModel.Handler = CommandHandler.Create<FileInfo, string, IHost, bool>(async (modelFile, repository, host, strict) =>
            {
                ILogger logger = GetLogger(host);
                OutputHeaders(logger);

                Parsing parsing = new Parsing(repository, logger);
                bool isValid;
                try
                {
                    isValid = await parsing.IsValidDtdlFileAsync(modelFile, strict);
                }
                catch (ResolutionException resolutionEx)
                {
                    logger.LogError(resolutionEx.Message);
                    return ReturnCodes.ResolutionError;
                }
                catch (ParsingException parsingEx)
                {
                    IList<ParsingError> errors = parsingEx.Errors;
                    string normalizedErrors = string.Empty;
                    foreach (ParsingError error in errors)
                    {
                        normalizedErrors += $"{error.Message}{Environment.NewLine}";
                    }

                    logger.LogError(normalizedErrors);

                    return ReturnCodes.ParserError;
                }
                catch (ResolverException resolverEx)
                {
                    logger.LogError(resolverEx.Message);
                    return ReturnCodes.ResolutionError;
                }
                catch (ValidationException validationEx)
                {
                    logger.LogError(validationEx.Message);
                    return ReturnCodes.ValidationError;
                }

                return isValid ? ReturnCodes.Success : ReturnCodes.ValidationError;
            });

            return validateModel;
        }

        private static Command BuildImportModelCommand()
        {
            var modelFileOption = CommonOptions.ModelFile;
            modelFileOption.IsRequired = true; // Option is required for this command

            Command addModel = new Command("import")
            {
                modelFileOption,
                CommonOptions.LocalRepo
            };
            addModel.Description = "Adds a model to a local repository. " +
                "Validates Id's, dependencies and places model content in the proper location.";
            addModel.Handler = CommandHandler.Create<FileInfo, string, IHost>(async (modelFile, localRepository, host) =>
            {
                var returnCode = ReturnCodes.Success;
                ILogger logger = GetLogger(host);

                OutputHeaders(logger);

                if (localRepository == null)
                {
                    localRepository = Path.GetFullPath(".");
                }
                else if (Validations.IsRelativePath(localRepository))
                {
                    localRepository = Path.GetFullPath(localRepository);
                }

                DirectoryInfo repoDirInfo = new DirectoryInfo(localRepository);
                Parsing parsing = new Parsing(repoDirInfo.FullName, logger);
                try
                {
                    var newModels = await ModelImporter.ImportModels(modelFile, repoDirInfo, logger);
                    foreach (var model in newModels)
                    {
                        var validationResult = await parsing.IsValidDtdlFileAsync(model, false);

                        if (!validationResult)
                            returnCode = ReturnCodes.ValidationError;
                    }
                }
                catch (ValidationException validationEx)
                {
                    logger.LogError(validationEx.Message);
                    return ReturnCodes.ValidationError;
                }
                catch (IOException ioEx)
                {
                    logger.LogError(ioEx.Message);
                    return ReturnCodes.ImportError;
                }

                return returnCode;
            });

            return addModel;
        }
    }
}
