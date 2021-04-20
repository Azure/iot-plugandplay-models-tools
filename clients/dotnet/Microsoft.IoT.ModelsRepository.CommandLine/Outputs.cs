using Azure.IoT.ModelsRepository;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class Outputs
    {
        public static readonly string ParserVersion = FileVersionInfo.GetVersionInfo(typeof(ModelParser).Assembly.Location).ProductVersion;
        public static readonly string RepositoryClientVersion = FileVersionInfo.GetVersionInfo(typeof(ModelsRepositoryClient).Assembly.Location).ProductVersion;
        public static readonly string CommandLineVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
        public static readonly string DebugHeader =
            $"ModelsRepositoryCommandLine/{CommandLineVersion} ModelsRepositoryClient/{RepositoryClientVersion} DTDLParser/{ParserVersion}";
        public static string DefaultErrorToken = "[Error]:";

        public static void WriteError(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"{DefaultErrorToken} {msg}");
            Console.ResetColor();
        }

        public static void WriteHeader()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Error.WriteLine(DebugHeader);
            Console.ResetColor();
        }

        public static void WriteOut(string content, ConsoleColor? color=null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.Out.WriteLine(content);

            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }

        public static void WriteDebug(string debug, ConsoleColor? color = null)
        {
            if (!color.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
            }

            Console.Error.WriteLine(debug);

            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }

        public static void WriteToFile(string filePath, string contents)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                UTF8Encoding utf8WithoutBom = new UTF8Encoding(false);
                File.WriteAllText(filePath, contents, utf8WithoutBom);
            }
        }

        public static void WriteToFile(FileInfo fileInfo, string contents)
        {
            if (fileInfo != null)
            {
                WriteToFile(fileInfo.FullName, contents);
            }
        }

        public static string FormatExpandedListAsJson(List<string> models)
        {
            // Due to model content already being serialized.
            string normalizedList = string.Join(',', models);
            string payload = $"[{normalizedList}]";

            // Ensures consistent format.
            using JsonDocument document = JsonDocument.Parse(payload, ParsingUtils.DefaultJsonParseOptions);
            return JsonSerializer.Serialize(document.RootElement, ParsingUtils.DefaultJsonSerializerOptions);
        }
    }
}
