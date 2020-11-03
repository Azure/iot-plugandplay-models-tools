using System.Collections.Generic;

namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    internal class StandardStrings
    {
        public static string GenericResolverError(string dtmi) 
            => $"Unable to resolve '{dtmi}'. ";

        public static string InvalidDtmiFormat(string dtmi) 
            => $"Input DTMI '{dtmi}' has an invalid format. ";

        public static string ClientInitWithFetcher(string scheme) 
            => $"Client initialized with {scheme} content fetcher. ";

        public static string SkippingPreProcessedDtmi(string dtmi) 
            => $"Already processed DTMI {dtmi}. Skipping...";

        public static string ProcessingDtmi(string dtmi) 
            => $"Processing DTMI '{dtmi}'. ";

        public static string DiscoveredDependencies(IList<string> dependencies) 
            => $"Discovered dependencies {string.Join(", ", dependencies)}. ";

        public static string IncorrectDtmiCasing(string expectedDtmi, string parsedDtmi) 
            => $"Retrieved model content has incorrect DTMI casing. Expected '{expectedDtmi}', parsed '{parsedDtmi}'. ";

        public static string FetchingContent(string path) 
            => $"Attempting to retrieve model content from '{path}'. ";

        public static string ErrorAccessLocalRepository(string repoPath) 
            => $"Local repository directory '{repoPath}' not found or not accessible. ";

        public static string ErrorAccessLocalRepositoryModel(string path) 
            => $"Model file '{path}' not found or not accessible in local repository. ";

        public static string ErrorAccessRemoteRepositoryModel(string path) 
            => $"Model uri '{path}' not found or not accessible in remote repository. ";
    }
}
