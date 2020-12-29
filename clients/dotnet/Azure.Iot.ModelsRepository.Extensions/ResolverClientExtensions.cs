using Microsoft.Azure.DigitalTwins.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Azure.Iot.ModelsRepository.Extensions
{
    public static class ResolverClientExtensions
    {
        public async static Task<IEnumerable<string>> ParserDtmiResolver(this ResolverClient client, IReadOnlyCollection<Dtmi> dtmis)
        {
            IEnumerable<string> dtmiStrings = dtmis.Select(s => s.AbsoluteUri);
            IDictionary<string, string> result = await client.ResolveAsync(dtmiStrings);
            return result.Values.ToList();
        }
    }
}