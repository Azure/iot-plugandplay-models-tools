using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.DigitalTwins.Validator.Exceptions;

namespace Azure.DigitalTwins.Validator
{
    public static partial class Validations
    {

        public async static Task<bool> ScanForReservedWords(this FileInfo fileInfo)
        {
            var reservedRegEx = new Regex("Microsoft|Azure", RegexOptions.IgnoreCase);

            var fileText = await File.ReadAllTextAsync(fileInfo.FullName);
            var ids = FindAllIds(fileText, (id) =>
            {
                if (reservedRegEx.IsMatch(id))
                {
                    return false;
                }
                return true;
            });
            var invalidIds = ids.Where(id => !id.Value);
            if (invalidIds.Any())
            {
                throw new ReservedWordException(invalidIds.Select(id => id.Key));
            }
            return true;
        }

        public static IEnumerable<KeyValuePair<string, bool>> FindAllIds(string fileText, Func<string, bool> validation)
        {
            var idRegex = new Regex("\\\"@id\\\":\\s?\\\"[^\\\"]*\\\",?");
            foreach (Match id in idRegex.Matches(fileText))
            {
                yield return new KeyValuePair<string, bool>(id.Value, validation(id.Value));
            }
        }
    }
}
