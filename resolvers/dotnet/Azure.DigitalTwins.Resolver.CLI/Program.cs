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
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Azure.DigitalTwins.Resolver.CLI
{
    class Program
    {
        private static readonly string _parserVersion = typeof(ModelParser).Assembly.GetName().Version.ToString();
        private static readonly string _resolverVersion = typeof(ResolverClient).Assembly.GetName().Version.ToString();

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
                Description = "Microsoft IoT Plug and Play Device Model Repository CLI"
            };

            root.Add(BuildShowCommand());
            root.Add(BuildResolveCommand());
            root.Add(BuildValidateCommand());

            return new CommandLineBuilder(root);
        }


        private static ResolverClient InitializeClient(string repository, ILogger logger)
        {
            ResolverClient client;
            client = Directory.Exists(repository) ?
                ResolverClient.FromLocalRegistry(repository, logger) : ResolverClient.FromRemoteRegistry(repository, logger);
            return client;
        }

        private static Command BuildShowCommand()
        {
            Command showModel = new Command("show")
            {
                CommonOptions.Dtmi(),
                CommonOptions.Repo(),
                CommonOptions.Output()
            };

            showModel.Description = "Shows the fully qualified path of an input dtmi. Does not evaluate existance of content.";
            showModel.Handler = CommandHandler.Create<string, string, IHost, string>(async (dtmi, repository, host, output) =>
            {
                IServiceProvider serviceProvider = host.Services;
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(typeof(Program));

                logger.LogInformation($"Resolver client version: {_resolverVersion}");
                logger.LogInformation($"Using repository location: {repository}");

                ResolverClient client = InitializeClient(repository, logger);
                string qualifiedPath = client.GetPath(dtmi);
                await Console.Out.WriteLineAsync(qualifiedPath);

                if (!string.IsNullOrEmpty(output))
                {
                    logger.LogInformation($"Writing result to file '{output}'");
                    await File.WriteAllTextAsync(output, qualifiedPath, Encoding.UTF8);
                }

                return ReturnCodes.Success;
            });

            return showModel;
        }

        private static Command BuildResolveCommand()
        {
            Command resolveModel = new Command("resolve")
            {
                CommonOptions.Dtmi(),
                CommonOptions.Repo(),
                CommonOptions.Output()
            };

            resolveModel.Description = "Retrieve a model and its dependencies by dtmi using the target repository for model resolution.";
            resolveModel.Handler = CommandHandler.Create<string, string, IHost, string>(async (dtmi, repository, host, output) =>
            {
                IServiceProvider serviceProvider = host.Services;
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(typeof(Program));

                logger.LogInformation($"Resolver client version: {_resolverVersion}");
                IDictionary<string, string> result;
                try
                {
                    logger.LogInformation($"Using repository location: {repository}");
                    result = await InitializeClient(repository, logger).ResolveAsync(dtmi);
                }
                catch (ResolverException resolverEx)
                {
                    logger.LogError(resolverEx.Message);
                    return ReturnCodes.ResolutionError;
                }

                List<string> resultList = result.Values.ToList();
                string normalizedList = string.Join(',', resultList);
                string payload = "[" + string.Join(',', normalizedList) + "]";
                await Console.Out.WriteLineAsync(payload);

                if (!string.IsNullOrEmpty(output))
                {
                    logger.LogInformation($"Writing result to file '{output}'");
                    await File.WriteAllTextAsync(output, payload, Encoding.UTF8);
                }

                return ReturnCodes.Success;
            });

            return resolveModel;
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
                CommonOptions.Repo()
            };

            validateModel.Description = "Validates a model using the Digital Twins model parser. Uses the target repository for model resolution.";
            validateModel.Handler = CommandHandler.Create<FileInfo, string, IHost>(async (modelFile, repository, host) =>
            {
                // TODO: DRY
                IServiceProvider serviceProvider = host.Services;
                ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                ILogger logger = loggerFactory.CreateLogger(typeof(Program));

                ResolverClient client = InitializeClient(repository, logger);

                logger.LogInformation($"Parser version: {_parserVersion}, Resolver client version: {_resolverVersion}");
                ModelParser parser = new ModelParser
                {
                    Options = new HashSet<ModelParsingOption>() { ModelParsingOption.StrictPartitionEnforcement }
                };

                parser.DtmiResolver = client.ParserDtmiResolver;

                try
                {
                    logger.LogInformation($"Repository location: {repository}");
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
                catch (ResolverException resolverEx)
                {
                    logger.LogError(resolverEx.Message);
                    return ReturnCodes.ResolutionError;
                }

                return ReturnCodes.Success;
            });

            return validateModel;
        }
    }
}
