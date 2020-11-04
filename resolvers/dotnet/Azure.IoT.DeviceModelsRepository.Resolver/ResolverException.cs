using System;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    public class ResolverException : Exception
    {
        public ResolverException(string dtmi) : 
            base(string.Format(StandardStrings.GenericResolverError, dtmi))
        {
        }

        public ResolverException(string dtmi, string message) : 
            base($"{string.Format(StandardStrings.GenericResolverError, dtmi)}{message}")
        {
        }

        public ResolverException(string dtmi, Exception innerException) :
            base(string.Format(StandardStrings.GenericResolverError, dtmi), innerException)
        {
        }

        public ResolverException(string dtmi, string message, Exception innerException) : 
            base($"{string.Format(StandardStrings.GenericResolverError, dtmi)}{message}", innerException)
        {
        }
    }
}
