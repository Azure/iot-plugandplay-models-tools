using Azure.Core.Diagnostics;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    class Program
    {
        static async Task<int> Main(string[] args) => await GetCommandLine().UseDefaults().Build().InvokeAsync(args);

        private static CommandLineBuilder GetCommandLine()
        {
            RootCommand root = new RootCommand("parent")
            {
                Description = $"Microsoft IoT Models Repository CommandLine v{Outputs.CommandLineVersion}"
            };

            root.Add(BuildExportCommand());
            root.Add(BuildValidateCommand());
            root.Add(BuildImportModelCommand());
            root.Add(BuildRepoIndexCommand());
            root.Add(BuildRepoExpandCommand());

            root.AddGlobalOption(CommonOptions.Debug);
            root.AddGlobalOption(CommonOptions.Silent);

            CommandLineBuilder builder = new CommandLineBuilder(root);
            builder.UseMiddleware(async (context, next) =>
            {
                AzureEventSourceListener listener = null;
                try
                {
                    if (context.ParseResult.Tokens.Any(x => x.Type == TokenType.Option && x.Value == "--debug"))
                    {
                        Outputs.WriteHeader();
                        listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Verbose);
                        Outputs.WriteDebug(context.ParseResult.ToString());
                    }

                    if (context.ParseResult.Tokens.Any(x => x.Type == TokenType.Option && x.Value == "--silent"))
                    {
                        System.Console.SetOut(TextWriter.Null);
                    }

                    await next(context);
                }
                finally
                {
                    if (listener != null)
                    {
                        listener.Dispose();
                    }
                }
            });

            return builder;
        }

        private static Command BuildExportCommand()
        {
            Command exportModelCommand = new Command("export")
            {
                CommonOptions.Dtmi,
                CommonOptions.ModelFile,
                CommonOptions.Repo,
                CommonOptions.OutputFile
            };

            exportModelCommand.Description =
                "Exports a model producing the model and its dependency chain in an expanded format. " +
                "The target repository is used for model resolution.";
            exportModelCommand.Handler = CommandHandler.Create<string, FileInfo, string, FileInfo>(Handlers.Export);

            return exportModelCommand;
        }

        private static Command BuildValidateCommand()
        {
            var modelFileOption = CommonOptions.ModelFile;
            modelFileOption.IsRequired = true; // Option is required for this command

            Command validateModelCommand = new Command("validate")
            {
                modelFileOption,
                CommonOptions.Repo,
                CommonOptions.Strict,
            };

            validateModelCommand.Description =
                "Validates the DTDL model contained in a file. When validating a single model object " +
                "the target repository is used for model resolution. When validating an array of models only the array " +
                "contents is used for resolution.";

            validateModelCommand.Handler =
                CommandHandler.Create<FileInfo, string, bool>(Handlers.Validate);

            return validateModelCommand;
        }

        private static Command BuildImportModelCommand()
        {
            var modelFileOption = CommonOptions.ModelFile;
            modelFileOption.IsRequired = true; // Option is required for this command

            Command importModelCommand = new Command("import")
            {
                modelFileOption,
                CommonOptions.LocalRepo,
                CommonOptions.Strict,
            };
            importModelCommand.Description =
                "Imports models from a model file into the local repository. The local repository is used for model resolution.";
            importModelCommand.Handler = CommandHandler.Create<FileInfo, DirectoryInfo, bool>(Handlers.Import);

            return importModelCommand;
        }

        private static Command BuildRepoIndexCommand()
        {
            var outputFileOption = CommonOptions.OutputFile;
            outputFileOption.IsRequired = true;

            Command repoIndexCommand= new Command("index")
            {
                CommonOptions.LocalRepo,
                outputFileOption
            };
            repoIndexCommand.Description =
                "Builds a model index file from the state of a target local models repository.";
            repoIndexCommand.Handler = CommandHandler.Create<DirectoryInfo, FileInfo>(Handlers.RepoIndex);

            return repoIndexCommand;
        }

        private static Command BuildRepoExpandCommand()
        {
            Command repoExpandCommand = new Command("expand")
            {
                CommonOptions.LocalRepo
            };
            repoExpandCommand.Description =
                "For each model in a local repository, generate expanded model files and insert them in-place. " +
                "The expanded version of a model includes the model with its full model dependency chain.";

            repoExpandCommand.Handler = CommandHandler.Create<DirectoryInfo>(Handlers.RepoExpand);

            return repoExpandCommand;
        }
    }
}
