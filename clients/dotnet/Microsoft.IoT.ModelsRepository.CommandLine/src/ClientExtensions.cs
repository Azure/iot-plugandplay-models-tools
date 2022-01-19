// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.IoT.ModelsRepository;
using Microsoft.Azure.DigitalTwins.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.IoT.ModelsRepository.Extensions
{
    public static class ClientExtensions
    {
        public async static Task<IEnumerable<string>> ParserDtmiResolver(this ModelsRepositoryClient client, IReadOnlyCollection<Dtmi> dtmis)
        {
            IEnumerable<string> dtmiStrings = dtmis.Select(s => s.AbsoluteUri);
            var modelDefinitions = new List<string>();
            foreach (var dtmi in dtmiStrings)
            {
                ModelResult result = await client.GetModelAsync(dtmi, ModelDependencyResolution.Disabled);
                modelDefinitions.Add(result.Content[dtmi]);
            }

            return modelDefinitions;
        }
    }
}