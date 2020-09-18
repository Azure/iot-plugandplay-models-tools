using System.IO;

namespace Microsoft.Azure.DigitalTwins.Resolver
{
    public class Utility
    {
        public static string DtmiToFilePath(string dtmi, string basePath=null)
        {
            // Lookups are case insensitive
            dtmi = dtmi.ToLower();

            // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
            string[] splitDtmi = dtmi.Split(':');
            string modelFilePath = Path.Combine(splitDtmi);
            modelFilePath = modelFilePath.Replace(';', '-');
            modelFilePath += ".json";

            if (basePath != null)
            {
                modelFilePath = Path.Combine(basePath, modelFilePath);
            }

            return modelFilePath;
        }

        public static string DtmiToRemotePath(string dtmi, string endpoint)
        {
            // Lookups are case insensitive
            dtmi = dtmi.ToLower();

            // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
            string[] splitDtmi = dtmi.Split(':');
            string remoteModelPath = string.Join('/', splitDtmi);
            remoteModelPath = remoteModelPath.Replace(';', '-');
            remoteModelPath += ".json";

            if (!endpoint.EndsWith('/'))
            {
                endpoint = endpoint += "/";
            }

            return $@"{endpoint}{remoteModelPath}";
        }
    }
}
