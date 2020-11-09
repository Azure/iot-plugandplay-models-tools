using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Azure.IoT.DeviceModelsRepository.Resolver;

namespace Azure.IoT.DeviceModelsRepository.CLI
{
    public static class Validations
    {
        public static string EnsureValidModelFilePath(string modelFilePath, string modelContent, string repository)
        {
            if (IsRelativePath(repository))
                repository = Path.GetFullPath(repository);

            string rootId = new Parsing(null).GetRootId(modelContent);
            string modelPath = Path.GetFullPath(DtmiConventions.DtmiToQualifiedPath(rootId, repository));
            Uri targetModelPathUri = new Uri(modelPath);
            Uri modelFilePathUri = new Uri(modelFilePath);

            if (targetModelPathUri.AbsolutePath != modelFilePathUri.AbsolutePath)
            {
                return Path.GetFullPath(targetModelPathUri.AbsolutePath);
            }

            return null;
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
            string dtmiNamespace = GetDtmiNamespace(new Parsing(null).GetRootId(fileText));

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

        public static bool IsRemoteEndpoint(string repositoryPath)
        {
            bool validUri = Uri.TryCreate(repositoryPath, UriKind.Absolute, out Uri testUri);
            return validUri && testUri != null && testUri.Scheme != "file";
        }
    }
}
