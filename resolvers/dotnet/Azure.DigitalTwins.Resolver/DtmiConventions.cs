namespace Azure.DigitalTwins.Resolver
{
    public class DtmiConventions
    {
        public static string ToPath(string dtmi, string basePath)
        {
            // Lookups are case insensitive
            dtmi = dtmi.ToLowerInvariant();
            string dtmiPath = $"{dtmi.Replace(":", "/").Replace(";", "-")}.json";

            if (!basePath.EndsWith("/"))
            {
                basePath += "/";
            }

            return $"{basePath}{dtmiPath}";
        }
    }
}
