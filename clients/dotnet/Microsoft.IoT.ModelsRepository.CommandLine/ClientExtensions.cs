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
            IDictionary<string, string> result = await client.GetModelsAsync(dtmiStrings);
            return result.Values.ToList();
        }
    }
}