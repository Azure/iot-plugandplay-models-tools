using Azure.DigitalTwins.Resolver.Extensions;
using Azure.DigitalTwins.Validator;
using Azure.DigitalTwins.Validator.Exceptions;
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver.CLI
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


        private static ResolverClient InitializeClient(string repository, ILogger logger)
        {
            ResolverClient client;
            client = Directory.Exists(repository) ?
                ResolverClient.FromLocalRepository(repository, logger) : ResolverClient.FromRemoteRepository(repository, logger);
            return client;
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

                logger.LogInformation($"Resolver client version: {_resolverVersion}");
                IDictionary<string, string> result;

                //check that we have either model file or dtmi
                if (string.IsNullOrWhiteSpace(dtmi) && modelFile == null)
                {
                    logger.LogError("Either dtmi or model-file must be specified");
                    return ReturnCodes.InvalidArguments;
                }
                try
                {
                    if (string.IsNullOrWhiteSpace(dtmi))
                    {
                        dtmi = GetRootDtmiFromFile(modelFile);
                    }
                    logger.LogInformation($"Using repository location: {repository}");
                    result = await InitializeClient(repository, logger).ResolveAsync(dtmi);
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

        private static string GetRootDtmiFromFile(FileInfo fileName)
        {
            var jsonDocument = JsonDocument.Parse(File.ReadAllText(fileName.FullName));
            var idElement = jsonDocument.RootElement.GetProperty("@id");
            return idElement.GetString();
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

            validateModel.Description = "Validates a model using the Digital Twins model parser. Uses the target repository for model resolution.";
            validateModel.Handler = CommandHandler.Create<FileInfo, string, IHost, bool>(async (modelFile, repository, host, strict) =>
            {
                ILogger logger = GetLogger(host);

                ModelParser parser = GetParser(repository, logger);

                try
                {
                    return await validateFile(modelFile, repository, strict, logger, parser);
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
            });

            return validateModel;
        }

        private static ModelParser GetParser(string repository, ILogger logger)
        {
            ResolverClient client = InitializeClient(repository, logger);

            logger.LogInformation($"Parser version: {_parserVersion}, Resolver client version: {_resolverVersion}");
            ModelParser parser = new ModelParser
            {
                Options = new HashSet<ModelParsingOption>() { ModelParsingOption.StrictPartitionEnforcement }
            };

            parser.DtmiResolver = client.ParserDtmiResolver;
            return parser;
        }

        private static async Task<int> validateFile(FileInfo modelFile, string repository, bool strict, ILogger logger, ModelParser parser)
        {
            logger.LogInformation($"Repository location: {repository}");
            await parser.ParseAsync(new string[] { File.ReadAllText(modelFile.FullName) });
            if (strict)
            {
                return await modelFile.Validate() ? ReturnCodes.Success : ReturnCodes.ValidationError;
            }
            return ReturnCodes.Success;
        }

        private static Command BuildImportModelCommand()
        {
            var modelFileOption = CommonOptions.ModelFile;
            modelFileOption.IsRequired = true; // Option is required for this command

            Command addModel = new Command("import")
            {
                modelFileOption,
                CommonOptions.LocalRepo,
                CommonOptions.Force
            };
            addModel.Description = "Adds a model to the repo. Validates ids, dependencies and set the right folder/file name";
            addModel.Handler = CommandHandler.Create<FileInfo, DirectoryInfo, bool, IHost>(async (modelFile, repository, force, host) =>
            {
                var returnCode = ReturnCodes.Success;
                ILogger logger = GetLogger(host);
                var parser = GetParser(repository.FullName, logger);
                try
                {
                    var newModels = await ModelImporter.importModels(modelFile, repository, force, logger);
                    foreach(var model in newModels)
                    {
                        var validationResult = await validateFile(model, repository.FullName, true, logger, parser);
                        if (validationResult != ReturnCodes.Success)
                        {
                            returnCode = validationResult;
                        }
                    }
                }
                catch (ValidationException validationEx)
                {
                    logger.LogError(validationEx.Message);
                    return ReturnCodes.ValidationError;
                }
                return returnCode;


            });
            return addModel;
        }
    }
}
