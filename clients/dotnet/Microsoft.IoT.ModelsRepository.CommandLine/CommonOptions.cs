using Azure.IoT.ModelsRepository;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class CommonOptions
    {
        public static Option<string> Dtmi
        {
            get
            {
                Option<string> dtmiOption = new Option<string>(
                    alias: "--dtmi",
                    description: "Digital Twin Model Identifier. Example: \"dtmi:com:example:Thermostat;1\".");

                dtmiOption.AddValidator(option =>
                {
                    string value = option.GetValueOrDefault<string>();
                    if (!DtmiConventions.IsValidDtmi(value))
                    {
                        return $"Invalid dtmi format '{value}'.";
                    }
                    return null;
                });

                dtmiOption.Argument = new Argument<string>
                {
                    Arity = ArgumentArity.ZeroOrOne
                };

                return dtmiOption;
            }
        }

        public static Option<string> Repo
        {
            get
            {
                Option<string> repoOption = new Option<string>(
                    alias: "--repo",
                    description: "Models repository location. Supports remote endpoint or local directory.",
                    getDefaultValue: () => new ModelsRepositoryClient().RepositoryUri.AbsoluteUri);

                return repoOption;
            }
        }

        public static Option<DirectoryInfo> LocalRepo
        {
            get
            {
                return new Option<DirectoryInfo>(
                  alias: "--local-repo",
                  description: "Local models repository path. If no path is provided the current working directory is used.",
                  getDefaultValue: () => null)
                {
                    Argument = new Argument<DirectoryInfo>()
                };
            }
        }

        public static Option<FileInfo> OutputFile
        {
            get
            {
                return new Option<FileInfo>(
                    aliases: new string[] { "--output-file", "-o" },
                    description: "Desired file path to write result contents. ",
                    getDefaultValue: () => null
                    );
            }
        }

        public static Option<FileInfo> ModelFile
        {
            get
            {
                return new Option<FileInfo>(
                    aliases: new string[] { "-m", "--model-file" },
                    description: "Path to file containing Digital Twins model content.")
                {
                    Argument = new Argument<FileInfo>().ExistingOnly()
                };
            }
        }

        public static Option<bool> Silent
        {
            get
            {
                return new Option<bool>(
                    alias: "--silent",
                    description: "Silences command output on standard out.",
                    getDefaultValue: () => false)
                {
                    Argument = new Argument<bool>
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    },
                };
            }
        }

        public static Option<bool> Strict
        {
            get
            {
                return new Option<bool>(
                    alias: "--strict",
                    description: "Runs additional verifications for a model including file paths and DTMI scoping.",
                    getDefaultValue: () => false)
                {
                    Argument = new Argument<bool>
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    },
                };
            }
        }

        public static Option<bool> Debug
        {
            get
            {
                return new Option<bool>(
                    alias: "--debug",
                    description: "Shows additional logs for debugging.",
                    getDefaultValue: () => false)
                {
                    Argument = new Argument<bool>
                    {
                        Arity = ArgumentArity.ZeroOrOne
                    },
                };
            }
        }
    }
}
