using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Azure.IoT.DeviceModelsRepository.Resolver;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    public static class Validations
    {
        public static bool IsValidDtmiPath(string fullPath)
        {
            var filePathRegex = new Regex("dtmi[\\\\\\/](?:_+[a-z0-9]|[a-z])(?:[a-z0-9_]*[a-z0-9])?(?:[\\\\\\/](?:_+[a-z0-9]|[a-z])(?:[a-z0-9_]*[a-z0-9])?)*-[1-9][0-9]{0,8}\\.json$");

            if (!filePathRegex.IsMatch(fullPath))
            {
                return false;
            }
            return true;
        }

        public static List<string> ScanIdsForReservedWords(string fileText)
        {
            List<string> badIds = new List<string>();
            var reservedRegEx = new Regex("Microsoft|Azure", RegexOptions.IgnoreCase);

            FindAllIds(fileText, (id) =>
            {
                if (reservedRegEx.IsMatch(id))
                    badIds.Add(id);
            });

            return badIds;
        }

        public static List<string> EnsureSubDtmiNamespace(string fileText)
        {
            List<string> badIds = new List<string>();
            ModelMetadata metadata = new ModelQuery(fileText).GetMetadata();
            string dtmiNamespace = GetDtmiNamespace(metadata.Id);

            FindAllIds(fileText, (id) =>
            {
                if (!id.StartsWith(dtmiNamespace))
                    badIds.Add(id);
            });

            return badIds;
        }

        private static void FindAllIds(string fileText, Action<string> validation)
        {
            var idRegex = new Regex("\\\"@id\\\":\\s?\\\"[^\\\"]*\\\",?");
            foreach (Match id in idRegex.Matches(fileText))
            {
                // return just the value without "@id" and quotes
                var idValue = Regex.Replace(Regex.Replace(id.Value, "\\\"@id\\\":\\s?\"", ""), "\",?", "");
                validation(idValue);
            }
        }

        public static string GetDtmiNamespace(string rootId)
        {
            var versionRegex = new Regex(";[1-9][0-9]{0,8}$");
            return versionRegex.Replace(rootId, "");
        }

        public static bool IsRelativePath(string repositoryPath)
        {
            bool validUri = Uri.TryCreate(repositoryPath, UriKind.Relative, out Uri testUri);
            return validUri && testUri != null;
        }
    }
}
