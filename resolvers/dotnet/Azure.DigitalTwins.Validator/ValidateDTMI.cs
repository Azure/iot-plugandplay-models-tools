using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.DigitalTwins.Validator.Exceptions;


namespace Azure.DigitalTwins.Validator
{
    public static partial class Validations
    {

        public async static Task<bool> ValidateDTMI(this FileInfo fileInfo)
        {
            var dtmiRegex = new Regex("^dtmi:(?:_+[A-Za-z0-9]|[A-Za-z])(?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::(?:_+[A-Za-z0-9]|[A-Za-z])(?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
            var versionRegex = new Regex(";[1-9][0-9]{0,8}$");

            var fileText = await File.ReadAllTextAsync(fileInfo.FullName);
            var model = JsonDocument.Parse(fileText).RootElement;
            JsonElement rootId;
            if(model.TryGetProperty("@id", out rootId)){

            } else {
                throw new MissingDTMIException(fileInfo);
            }

            var dtmiNamespace = versionRegex.Replace(rootId.GetString(), "");
            var exceptions = new List<Exception>();

            var ids = FindAllIds(fileText, (id) => {
                if(!dtmiRegex.IsMatch(id)) {
                    exceptions.Add(new InvalidDTMIException(id));
                    return false;
                }
                if(!id.Contains(dtmiNamespace)) {
                    exceptions.Add(new InvalidSubDTMIException(id));
                    return false;
                }
                return true;
            });
            var invalidIds = ids.Where(id => !id.Value);
            if(invalidIds.Any()) {
                throw new InvalidDTMIException(invalidIds.Select(id => id.Key));
            }
            return true;
        }
    }
}
