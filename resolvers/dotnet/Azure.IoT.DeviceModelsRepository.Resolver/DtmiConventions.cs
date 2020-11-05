using System;
using System.Text.RegularExpressions;

﻿namespace Azure.IoT.DeviceModelsRepository.Resolver
{
    public class DtmiConventions
    {
        public static bool IsDtmi(string dtmi) => !string.IsNullOrEmpty(dtmi) && new Regex(@"^dtmi:[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::[A-Za-z](?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$").IsMatch(dtmi);
        public static string DtmiToPath(string dtmi) => IsDtmi(dtmi) ? $"{dtmi.ToLowerInvariant().Replace(":", "/").Replace(";", "-")}.json" : null;

        public static string DtmiToQualifiedPath(string dtmi, string basePath, bool fromExpanded = false)
        {
            string dtmiPath = DtmiToPath(dtmi);
            if (dtmiPath == null)
                throw new ArgumentException(string.Format(StandardStrings.InvalidDtmiFormat, dtmi));

            if (!basePath.EndsWith("/"))
                basePath += "/";

            string fullyQualifiedPath = $"{basePath}{dtmiPath}";

            if (fromExpanded)
                fullyQualifiedPath = fullyQualifiedPath.Replace(".json", ".expanded.json");

            return fullyQualifiedPath;
        }
    }
}
