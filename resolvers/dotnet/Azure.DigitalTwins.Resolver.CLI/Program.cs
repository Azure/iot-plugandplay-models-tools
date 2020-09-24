using Microsoft.Azure.DigitalTwins.Parser;
using Azure.DigitalTwins.Resolver.Extensions;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Azure.DigitalTwins.Resolver.CLI
{
    class Program
    {
        private static readonly string _defaultRegistry = "https://devicemodeltest.azureedge.net/";
        private static readonly string _parserVersion = typeof(ModelParser).Assembly.GetName().Version.ToString();

        // Alternative to enum to avoid casting.
        public static class ReturnCodes
        {
            public const int Success = 0;
            public const int ResolutionError = 1;
            public const int ParserError = 2;
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
                Description = "Microsoft IoT Plug and Play Model Resolution CLI"
            };

            root.Add(BuildShowCommand());
            root.Add(BuildValidateCommand());

            return new CommandLineBuilder(root);
        }


        private static ResolverClient InitializeClient(string registry)
        {
            ResolverClient client;
            client = Directory.Exists(registry) ?
                ResolverClient.FromLocalRegistry(registry) : ResolverClient.FromRemoteRegistry(registry);
            return client;
        }

        private static Command BuildShowCommand()
        {
            Command showModel = new Command("show")
            {
                new Option<string>(
                    "--dtmi",
                    description: "Digital Twin Model Identifier. Example: dtmi:com:example:Thermostat;1"){
                    Argument = new Argument<string>
                    {
                        Arity = ArgumentArity.ExactlyOne,
                    },
                    IsRequired = true
                },
                new Option<string>(
                    "--registry",
                    description: "Model Registry location. Can be remote endpoint or local directory.",
                    getDefaultValue: () => _defaultRegistry
                    ),
            };

            showModel.Description = "Retrieve a model and its dependencies by dtmi using the target registry for model resolution.";
            showModel.Handler = CommandHandler.Create<string, string, IHost>(async (dtmi, registry, host) =>
            {
                IServiceProvider serviceProvider = host.Services;
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(typeof(Program));

                IDictionary<string, string> result;
                try
                {
                    logger.LogInformation($"Using registry location {registry}");
                    result = await InitializeClient(registry).ResolveAsync(dtmi);
                }
                catch (DirectoryNotFoundException dnfex)
                {
                    logger.LogError(dnfex.Message);
                    return ReturnCodes.ResolutionError;
                }
                catch (FileNotFoundException fnfex)
                {
                    logger.LogError(fnfex.Message);
                    return ReturnCodes.ResolutionError;
                }
                catch (HttpRequestException httpex)
                {
                    logger.LogError(httpex.Message);
                    return ReturnCodes.ResolutionError;
                }

                List<string> resultList = result.Values.ToList();
                string normalizedList = string.Join(',', resultList);
                await Console.Out.WriteLineAsync("[" + string.Join(',', normalizedList) + "]");

                return ReturnCodes.Success;
            });

            return showModel;
        }

        private static Command BuildValidateCommand()
        {
            Command validateModel = new Command("validate")
            {
                new Option<FileInfo>(
                    "--model-file",
                    description: "Path to file containing Digital Twins model content."){
                    Argument = new Argument<FileInfo>().ExistingOnly(),
                    IsRequired = true
                },
                new Option<string>(
                    "--registry",
                    description: "Model Registry location. Can be remote endpoint or local directory.",
                    getDefaultValue: () => _defaultRegistry
                    ),
            };

            validateModel.Description = "Validates a model using the Digital Twins parser and target registry for model resolution.";
            validateModel.Handler = CommandHandler.Create<FileInfo, string, IHost>(async (modelFile, registry, host) =>
            {
                // TODO: DRY
                IServiceProvider serviceProvider = host.Services;
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(typeof(Program));

                ResolverClient client = InitializeClient(registry);

                logger.LogInformation($"Parser version: {_parserVersion}");
                ModelParser parser = new ModelParser
                {
                    Options = new HashSet<ModelParsingOption>() { ModelParsingOption.StrictPartitionEnforcement }
                };

                parser.DtmiResolver = client.ParserDtmiResolver;

                try
                {
                    logger.LogInformation($"Registry location: {registry}");
                    await parser.ParseAsync(new string[] { File.ReadAllText(modelFile.FullName) });
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
                    foreach(ParsingError error in errors)
                    {
                        normalizedErrors += $"{error.Message}{Environment.NewLine}";
                    }

                    logger.LogError(normalizedErrors);

                    return ReturnCodes.ParserError;
                }
                catch (DirectoryNotFoundException dnfex)
                {
                    logger.LogError(dnfex.Message);
                    return ReturnCodes.ResolutionError;
                }
                catch (FileNotFoundException fnfex)
                {
                    logger.LogError(fnfex.Message);
                    return ReturnCodes.ResolutionError;
                }
                catch (HttpRequestException httpex)
                {
                    logger.LogError(httpex.Message);
                    return ReturnCodes.ResolutionError;
                }

                return ReturnCodes.Success;
            });

            return validateModel;
        }
    }
}
