namespace Azure.DigitalTwins.Resolver
{
    public class DtmiConventions
    {
        public static string ToPath(string dtmi, string basePath)
        {
            // Lookups are case insensitive
            dtmi = dtmi.ToLowerInvariant();

            // dtmi:com:example:Thermostat;1 -> dtmi/com/example/thermostat-1.json
            string[] splitDtmi = dtmi.Split(':');
            string remoteModelPath = string.Join('/', splitDtmi);
            remoteModelPath = remoteModelPath.Replace(';', '-');
            remoteModelPath += ".json";

            if (!basePath.EndsWith('/'))
            {
                basePath += "/";
            }

            return $@"{basePath}{remoteModelPath}";
        }
    }
}
