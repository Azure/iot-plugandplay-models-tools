using System;

namespace Azure.DigitalTwins.Resolver
{
    public class ResolverException : Exception
    {
        public ResolverException(string dtmi) : 
            base($"Unable to resolve '{dtmi}'")
        {
        }

        public ResolverException(string dtmi, string message) : 
            base($"Unable to resolve '{dtmi}'. {message}")
        {
        }

        public ResolverException(string dtmi, string message, Exception innerException) : 
            base($"Unable to resolve '{dtmi}'. {message}", innerException)
        {
        }
    }
}
