using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    public interface IResolverClient
    {
        Task<IDictionary<string, string>> ResolveAsync(string dtmi);

        Task<IDictionary<string, string>> ResolveAsync(IEnumerable<string> dtmis);
    }
}