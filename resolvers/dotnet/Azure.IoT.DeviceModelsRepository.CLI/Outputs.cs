using Azure.IoT.DeviceModelsRepository.Resolver;
using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    internal class Outputs
    {
        public static readonly string ParserVersion = typeof(ModelParser).Assembly.GetName().Version.ToString();
        public static readonly string ResolverVersion = typeof(ResolverClient).Assembly.GetName().Version.ToString();
        public static readonly string CliVersion = typeof(Program).Assembly.GetName().Version.ToString();
        public static readonly string StandardHeader = $"dmr-client/{CliVersion} parser/{ParserVersion} resolver/{ResolverVersion}";

        public async static Task WriteErrorAsync(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Error.WriteLineAsync($"ERROR: {msg}");
            Console.ResetColor();
        }

        public async static Task WriteHeaderAsync()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            await Console.Error.WriteLineAsync(StandardHeader);
            Console.ResetColor();
        }

        public async static Task WriteOutAsync(string content, ConsoleColor? color=null)
        {
            if (color.HasValue)
            {
                Console.ForegroundColor = color.Value;
            }

            await Console.Out.WriteLineAsync(content);

            if (color.HasValue)
            {
                Console.ResetColor();
            }
        }

        public async static Task WriteDebugAsync(string debug, ConsoleColor? color = null)
        {
            if (!color.HasValue)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
            }

            await Console.Error.WriteLineAsync(debug);
            Console.ResetColor();
        }
    }
}
