
using Microsoft.Azure.DigitalTwins.Parser;
using Azure.DigitalTwins.Resolver;
using Azure.DigitalTwins.Resolver.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace dtdl2_validator
{
    class ModelValidationService
    {
        readonly ILogger log;
        readonly IConfiguration config;

        public ModelValidationService(IConfiguration configuration, ILogger logger)
        {
            this.log = logger;
            this.config = configuration;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            (string input, string resolverName) = ReadConfiguration(config);
            PrintHeader(input, resolverName);
            await ValidateAsync(input, resolverName);
        }

        private async Task ValidateAsync(string input, string resolverName)
        {
            ModelParser parser = new ModelParser();
            parser.Options = new HashSet<ModelParsingOption>() { ModelParsingOption.StrictPartitionEnforcement };
            ConfigureResolver(parser, resolverName);

            try
            {
                var parserResult = await parser.ParseAsync(new string[] { File.ReadAllText(input) });
                Console.WriteLine("Resolution completed");
                foreach (var item in parserResult.Values)
                {
                    this.log.LogTrace(item.Id.AbsoluteUri);
                }
                Console.WriteLine($"\nValidation Passed: {input}");
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                log.LogError(ex, "DTDL Parser Exception");
            }

        }

        private void ConfigureResolver(ModelParser parser, string resolverName)
        {
            ResolverClient resolver;
            if (resolverName == "local")
            {
                string baseFolder;
                baseFolder = config.GetValue<string>("baseFolder");

                if (string.IsNullOrEmpty(baseFolder))
                {
                    log.LogInformation("Local registry baseFolder config not found, using default.");
                    baseFolder = ".";
                }
                log.LogInformation($"Resolver configured with baseFolder='{baseFolder}'");

                resolver = ResolverClient.FromLocalRegistry(baseFolder);
                parser.DtmiResolver = resolver.ParserDtmiResolver;
            }
            else if(resolverName == "public")
            {
                string modelRepoUrl;
                modelRepoUrl = config.GetValue<string>("modelRepoUrl");
                if (string.IsNullOrEmpty(modelRepoUrl))
                {
                    log.LogInformation("Public registry modelRepoUrl config not found, using default.");
                    modelRepoUrl = "https://iotmodels.github.io/registry/";
                }
                log.LogInformation($"Resolver configured with modelRepoUrl={modelRepoUrl}");

                resolver = ResolverClient.FromRemoteRegistry(modelRepoUrl);
                parser.DtmiResolver = resolver.ParserDtmiResolver;
            }
        }

        private void PrintHeader(string input, string resolver)
        {
            Console.WriteLine("\n-----------------------------------");
            Console.WriteLine($"dtdl2-validator {input} {resolver}");
            Console.WriteLine($"version: {ThisAssemblyVersion} using parser: {ThisParserVersion}");
            Console.WriteLine("-----------------------------------");
        }

        (string, string) ReadConfiguration(IConfiguration config)
        {
            string input = config.GetValue<string>("f");
            string resolver = config.GetValue<string>("resolver"); ;

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Usage: dtdl2-validator /f=<dtdlFile.json> /resolver?=<public|local|none>");
                Environment.ExitCode = 2;
            }
            else
            {
                if (!File.Exists(input))
                {
                    Console.WriteLine($"File '{input}' not found");
                    Environment.ExitCode = 2;
                }
            }

            if (string.IsNullOrEmpty(resolver))
            {
                resolver = "public";
            }

            return (input, resolver);
        }

        static string ThisAssemblyVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();
        static string ThisParserVersion => typeof(ModelParser).Assembly.GetName().Version.ToString();
    }
}
