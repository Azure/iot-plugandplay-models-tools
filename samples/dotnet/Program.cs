using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace ResolutionSample
{
    class Program
    {
        static readonly string _repositoryEndpoint = "https://devicemodels.azure.com";
        static readonly HttpClient _httpClient;

        static Program()
        {
            // HttpClient is intended to be instantiated once per application, rather than per-use.
            _httpClient = new HttpClient();
        }

        static void Main(string[] args)
        {
            // Determine target DTMI for resolution.
            string dtmiTarget = args.Length == 0 ? "dtmi:azure:DeviceManagement:DeviceInformation;1" : args[0];

            Console.WriteLine($"Attempting to resolve: {dtmiTarget}");

            if (!IsValidDtmi(dtmiTarget))
                throw new ArgumentException($"Invalid DTMI input: {dtmiTarget}");

            // Apply model repository convention
            string dtmiPath = DtmiToPath(dtmiTarget);

            // Make request synchronously (for this exmaple)
            string modelContent = _httpClient.GetStringAsync($"{_repositoryEndpoint}/{dtmiPath}").Result;

            // Output string content to stdout
            Console.WriteLine(modelContent);
        }

        static string DtmiToPath(string dtmi)
        {
            // Lookups are case insensitive
            dtmi = dtmi.ToLowerInvariant();

            // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
            string[] splitDtmi = dtmi.Split(':');
            string modelPath = string.Join('/', splitDtmi);
            modelPath = modelPath.Replace(';', '-');
            modelPath += ".json";

            return modelPath;
        }

        static bool IsValidDtmi(string dtmi)
        {
            // Regex defined at https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
            Regex rx = new Regex(@"^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
            return rx.IsMatch(dtmi);
        }
    }
}
