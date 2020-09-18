﻿using Microsoft.Azure.DigitalTwins.Parser;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.DigitalTwins.Resolver.Extensions
{
    public static class ResolverClientExtensions
    {
        public async static Task<IEnumerable<string>> ParserDtmiResolver(this ResolverClient client, IReadOnlyCollection<Dtmi> dtmis)
        {
            IEnumerable<string> dtmiStrings = dtmis.Select(s => s.AbsoluteUri);
            Dictionary<string, string> result = await client.ResolveAsync(dtmiStrings);
            return result.Values.ToList();
        }
    }
}