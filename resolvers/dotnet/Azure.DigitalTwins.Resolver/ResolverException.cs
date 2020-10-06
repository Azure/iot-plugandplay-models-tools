using System;

namespace Azure.DigitalTwins.Resolver
{
    public class ResolverException : Exception
    {
        public ResolverException(string dtmi) : 
            base(StdStrings.GenericResolverError(dtmi))
        {
        }

        public ResolverException(string dtmi, string message) : 
            base($"{StdStrings.GenericResolverError(dtmi)}{message}")
        {
        }

        public ResolverException(string dtmi, string message, Exception innerException) : 
            base($"{StdStrings.GenericResolverError(dtmi)}{message}", innerException)
        {
        }
    }
}
