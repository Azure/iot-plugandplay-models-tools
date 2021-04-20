// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        const string _repositoryEndpoint = "https://devicemodels.azure.com";
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

            // Initiate first Resolve for the target dtmi to pass content to parser
            string dtmiContent = await Resolve(toParseDtmi);

            if (!string.IsNullOrEmpty(dtmiContent))
            {
                // Assign the callback
                ModelParser parser = new ModelParser
                {
                    DtmiResolver = ResolveCallback
                };
                await parser.ParseAsync(new List<string> { dtmiContent });
                Console.WriteLine("Parsing success!");
            } 
        }

        static async Task<IEnumerable<string>> ResolveCallback(IReadOnlyCollection<Dtmi> dtmis)
        {
            Console.WriteLine("ResolveCallback invoked!");
            List<string> result = new List<string>();

            foreach (Dtmi dtmi in dtmis)
            {
                string content = await Resolve(dtmi.ToString());
                result.Add(content);
            }

            return result;
        }

        static async Task<string> Resolve(string dtmi)
        {
            Console.WriteLine($"Attempting to resolve: {dtmi}");

            // Apply model repository convention
            string dtmiPath = DtmiToPath(dtmi.ToString());
            if (string.IsNullOrEmpty(dtmiPath)) 
            {
                Console.WriteLine($"Invalid DTMI: {dtmi}");
                return await Task.FromResult<string>(string.Empty);
            }
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
            {
                return null;
            }
            // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
            return $"/{dtmi.ToLowerInvariant().Replace(":", "/").Replace(";", "-")}.json";
        }

        static bool IsValidDtmi(string dtmi)
        {
            // Regex defined at https://github.com/Azure/digital-twin-model-identifier#validation-regular-expressions
            Regex rx = new Regex(@"^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
            return rx.IsMatch(dtmi);
        }
    }
}
