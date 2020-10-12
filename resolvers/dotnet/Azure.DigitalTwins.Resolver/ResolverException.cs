using System;

namespace Azure.DigitalTwins.Resolver
{
    public class ResolverException : Exception
    {
        public ResolverException(string dtmi) : 
            base(StandardStrings.GenericResolverError(dtmi))
        {
        }

        public ResolverException(string dtmi, string message) : 
            base($"{StandardStrings.GenericResolverError(dtmi)}{message}")
        {
        }

        public ResolverException(string dtmi, Exception innerException) :
            base(StandardStrings.GenericResolverError(dtmi), innerException)
        {
        }

        public ResolverException(string dtmi, string message, Exception innerException) : 
            base($"{StandardStrings.GenericResolverError(dtmi)}{message}", innerException)
        {
        }
    }
}
