using System.Collections.Generic;

namespace Azure.DigitalTwins.Resolver
{
    public class StandardStrings
    {
        public static string GenericResolverError(string dtmi)
        {
            return $"Unable to resolve '{dtmi}'. ";
        }

        public static string InvalidDtmiFormat(string dtmi)
        {
            return $"Input DTMI '{dtmi}' has an invalid format. ";
        }

        public static string ClientInitWithFetcher(string scheme)
        {
            return $"Client initialized with {scheme} content fetcher. ";
        }

        public static string SkippingPreProcessedDtmi(string dtmi)
        {
            return $"Already processed DTMI {dtmi}. Skipping...";
        }

        public static string ProcessingDtmi(string dtmi)
        {
            return $"Processing DTMI '{dtmi}'. ";
        }

        public static string DiscoveredDependencies(IList<string> dependencies)
        {
            return $"Discovered dependencies {string.Join(", ", dependencies)}. ";
        }

        public static string IncorrectDtmiCasing(string expectedDtmi, string parsedDtmi)
        {
            return $"Retrieved model content has incorrect DTMI casing. Expected {expectedDtmi}, parsed {parsedDtmi}. ";
        }

        public static string FetchingContent(string path)
        {
            return $"Attempting to retrieve model content from '{path}'. ";
        }

        public static string ErrorAccessLocalRepository(string repoPath)
        {
            return $"Local repository directory '{repoPath}' not found or not accessible. ";
        }

        public static string ErrorAccessLocalRepositoryModel(string path)
        {
            return $"Model file '{path}' not found or not accessible in local repository. ";
        }

        public static string ErrorAccessRemoteRepositoryModel(string path)
        {
            return $"Model uri '{path}' not found or not accessible in remote repository. ";
        }
    }
}
