using System.Diagnostics;
using System.IO;

namespace Azure.IoT.DeviceModelsRepository.CLI.Tests
{
    internal class ClientInvokator
    {
        readonly static string cliProjectPath = Path.GetFullPath(@"../../../../Azure.IoT.DeviceModelsRepository.CLI");

        public static (int, string, string) Invoke(string commandArgs)
        {
            ProcessStartInfo cmdsi = new ProcessStartInfo("dotnet")
            {
                Arguments = $"run -- {commandArgs}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = cliProjectPath
            };

            Process cmd = Process.Start(cmdsi);
            string standardOut = cmd.StandardOutput.ReadToEnd();
            string standardError = cmd.StandardError.ReadToEnd();

            cmd.WaitForExit(10000);

            return (cmd.ExitCode, standardOut, standardError);
        }
    }
}
