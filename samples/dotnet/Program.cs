using Microsoft.Azure.DigitalTwins.Parser;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ResolutionSample
{
    class Program
    {
        static readonly string _repositoryEndpoint = "https://devicemodeltest.azureedge.net";
        static readonly HttpClient _httpClient;

        static Program()
        {
            // HttpClient is intended to be instantiated once per application, rather than per-use.
            _httpClient = new HttpClient();
        }

        static async Task Main(string[] args)
        {
            // Target DTMI for resolution.
            string toParseDtmi = args.Length == 0 ? "dtmi:com:example:TemperatureController;1" : args[0];

            // Setup parser DtmiResolver callback in case more dependencies are needed
            Func<IReadOnlyCollection<Dtmi>, Task<IEnumerable<string>>> resolveCallback = async dtmis => {

                Console.WriteLine("resolveCallback invoked!");
                List<string> result = new List<string>();

                foreach(Dtmi dtmi in dtmis)
                {
                    string content = await Resolve(dtmi.ToString());
                    result.Add(content);
                }

                return result;
            };

            // Assign the callback
            ModelParser parser = new ModelParser
            {
                DtmiResolver = resolveCallback.Invoke
            };

            // Initiate first Resolve for the target dtmi to pass content to parser
            string dtmiContent = await Resolve(toParseDtmi);

            await parser.ParseAsync(new List<string> { dtmiContent });
            Console.WriteLine("Parsing success!");
        }

        static async Task<string> Resolve(string dtmi)
        {
            Console.WriteLine($"Attempting to resolve: {dtmi}");

            // Apply model repository convention
            string dtmiPath = DtmiToPath(dtmi.ToString());
            string fullyQualifiedPath = $"{_repositoryEndpoint}{dtmiPath}";

            Console.WriteLine($"Fully qualified model path: {fullyQualifiedPath}");

            // Make request
            string modelContent = await _httpClient.GetStringAsync(fullyQualifiedPath);

            // Output string model content to stdout
            Console.WriteLine("Received content...");
            Console.WriteLine(modelContent);

            return modelContent;
        }

        static string DtmiToPath(string dtmi)
        {
            if (!IsValidDtmi(dtmi))
                throw new ArgumentException($"Invalid DTMI input: {dtmi}");

            // Lookups are case insensitive
            dtmi = dtmi.ToLowerInvariant();

            // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
            string[] splitDtmi = dtmi.Split(':');
            string modelPath = string.Join('/', splitDtmi);
            modelPath = modelPath.Replace(';', '-');

            return $"/{modelPath}.json";
        }

        static bool IsValidDtmi(string dtmi)
        {
            // Regex defined at https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
            Regex rx = new Regex(@"^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
            return rx.IsMatch(dtmi);
        }
    }
}
