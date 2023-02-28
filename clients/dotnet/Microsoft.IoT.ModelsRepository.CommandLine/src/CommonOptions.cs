// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using Azure.IoT.ModelsRepository;
using DTDLParser;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class CommonOptions
    {
        public const int DefaultPageLimit = 2048;

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
                    if (!DTDLParser.Dtmi.TryCreateDtmi(value, out DTDLParser.Dtmi dtmi))
                    {
                        return $"Invalid dtmi format '{value}'.";
                    }
                    return null;
                });

                dtmiOption.Arity = ArgumentArity.ZeroOrOne;
                return dtmiOption;
            }
        }

        public static Option<string> Repo
        {
            get
            {
                Option<string> repoOption = new Option<string>(
                    aliases: new string[] { "--repo", "-r" },
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
                  aliases: new string[] { "--local-repo", "-r" },
                  description: "Local models repository path. If no path is provided the current working directory is used.",
                  getDefaultValue: () => null);
            }
        }

        public static Option<FileInfo> OutputFile
        {
            get
            {
                return new Option<FileInfo>(
                    aliases: new string[] { "--output-file", "-o" },
                    description: "Desired file path to write result contents.",
                    getDefaultValue: () => null
                    );
            }
        }

        public static Option<FileInfo> ModelFile
        {
            get
            {
                return new Option<FileInfo>(
                    aliases: new string[] { "--model-file", "-m" },
                    description: "Path to file containing Digital Twins model content.").ExistingOnly();
            }
        }

        public static Option<DirectoryInfo> ModelsDirectory
        {
            get
            {
                return new Option<DirectoryInfo>(
                    aliases: new string[] { "--directory" },
                    description: "Path to directory containing Digital Twins model content.").ExistingOnly();
            }
        }

        public static Option<string> ModelsDirectorySearchPattern
        {
            get
            {
                return new Option<string>(
                    aliases: new string[] { "--search-pattern" },
                    description: "The search string to match against the names of files in directory path. " +
                    "This parameter can contain a combination of valid literal path and wildcard " +
                    "(* and ?) characters, but it doesn't support regular expressions.",
                    getDefaultValue: () => "*.json");
            }
        }

        public static Option<int> PageLimit
        {
            get
            {
                Option<int> pageLimitOption = new Option<int>(
                    alias: "--page-limit",
                    getDefaultValue: () => DefaultPageLimit,
                    description: "Maximum models per page.");

                pageLimitOption.AddValidator(option =>
                {
                    int value = option.GetValueOrDefault<int>();
                    if (value < 1)
                    {
                        return "Minimum page limit is 1.";
                    }
                    return null;
                });

                return pageLimitOption;
            }
        }


      public static Option<bool> Force
      {
         get
         {
            return new Option<bool>(
                alias: "--force",
                description: "Forces import operation to override existing model file.",
                getDefaultValue: () => false)
            {
               Arity = ArgumentArity.ZeroOrOne
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
                    Arity = ArgumentArity.ZeroOrOne
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
                    Arity = ArgumentArity.ZeroOrOne
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
                    Arity = ArgumentArity.ZeroOrOne
                };
            }
        }

        public static Option<int> MaxDtdlVersion
        {
            get
            {
                return new Option<int>(
                    alias: "--maxDtdlVersion",
                    description: "Sets the maximum DTDL version accepted.",
                    getDefaultValue: () => Validations.DefaultMaxDtdlVersion)
                {
                    Arity = ArgumentArity.ZeroOrOne
                };

            }
        }
    }
}