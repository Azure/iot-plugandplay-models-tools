namespace Azure.DigitalTwins.Resolver
{
    public class DtmiConventions
    {
        public static string ToPath(string dtmi)
        {
            // Lookups are case insensitive
            return $"{dtmi.ToLowerInvariant().Replace(":", "/").Replace(";", "-")}.json";
        }

        public static string ToPath(string dtmi, string basePath, bool fromExpanded = false)
        {
            string dtmiPath = ToPath(dtmi);

            if (!basePath.EndsWith("/"))
                basePath += "/";

            string fullyQualifiedPath = $"{basePath}{dtmiPath}";

            if (fromExpanded)
                fullyQualifiedPath = fullyQualifiedPath.Replace(".json", ".expanded.json");

            return fullyQualifiedPath;
        }
    }
}
