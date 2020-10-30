using Azure.IoT.DeviceModelsRepository.Resolver;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    internal class Outputs
    {
        public static readonly string ParserVersion = typeof(ModelParser).Assembly.GetName().Version.ToString();
        public static readonly string ResolverVersion = typeof(ResolverClient).Assembly.GetName().Version.ToString();
        public static readonly string CliVersion = typeof(Program).Assembly.GetName().Version.ToString();

        public async static Task WriteErrorAsync(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Error.WriteLineAsync($"{Environment.NewLine}{msg}");
            Console.ResetColor();
        }

        public async static Task WriteHeadersAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            await Console.Out.WriteLineAsync($"dmr-client/{CliVersion} parser/{ParserVersion} resolver/{ResolverVersion}");
            Console.ResetColor();
        }

        public static void WriteOut(string content, ConsoleColor? color=null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }

            Console.Out.Write($"{content}{Environment.NewLine}");

            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }

        public async static Task WriteInputsAsync(string command, Dictionary<string, string> inputs)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            StringBuilder builder = new StringBuilder();
            builder.Append($"{command}");
            foreach (var item in inputs)
            {
                if (item.Value != null)
                {
                    builder.Append($" --{item.Key} {item.Value}");
                }
            }
            await Console.Out.WriteLineAsync($"{builder}{Environment.NewLine}");
            Console.ResetColor();
        }
    }
}
