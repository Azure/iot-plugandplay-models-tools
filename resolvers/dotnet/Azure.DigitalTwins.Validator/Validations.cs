using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Azure.DigitalTwins.Validator
{
    public static partial class Validations
    {
        public static IEnumerable<KeyValuePair<string, bool>> FindAllIds(string fileText, Func<string, bool> validation)
        {
            var idRegex = new Regex("\\\"@id\\\":\\s?\\\"[^\\\"]*\\\",?");
            foreach (Match id in idRegex.Matches(fileText))
            {
                // return just the value without "@id" and quotes
                var idValue = Regex.Replace(Regex.Replace(id.Value, "\\\"@id\\\":\\s?\"", ""), "\",?", "");
                yield return new KeyValuePair<string, bool>(idValue, validation(idValue));
            }
        }
    }
}