namespace Azure.DigitalTwins.Resolver
{
    public class DtmiConventions
    {
        public static string ToPath(string dtmi, string basePath, bool expanded = false)
        {
            // Lookups are case insensitive
            dtmi = dtmi.ToLowerInvariant();
            string dtmiPath = $"{dtmi.Replace(":", "/").Replace(";", "-")}.json";

            if (!basePath.EndsWith("/"))
                basePath += "/";

            string fullyQualifiedPath = $"{basePath}{dtmiPath}";

            if (expanded)
                fullyQualifiedPath = fullyQualifiedPath.Replace(".json", ".expanded.json");

            return fullyQualifiedPath;
        }
    }
}
