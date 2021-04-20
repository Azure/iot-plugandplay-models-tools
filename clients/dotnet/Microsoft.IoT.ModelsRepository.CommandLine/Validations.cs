using Azure.IoT.ModelsRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class Validations
    {
        public static string EnsureValidModelFilePath(string modelFilePath, string modelContent, string repository)
        {
            if (RepoProvider.IsRelativePath(repository))
            {
                repository = Path.GetFullPath(repository);
            }

            string rootId = ParsingUtils.GetRootId(modelContent);
            Uri targetModelPathUri = DtmiConventions.GetModelUri(rootId, new Uri(repository));
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
            string dtmiNamespace = GetDtmiNamespace(ParsingUtils.GetRootId(fileText));

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
    }
}
