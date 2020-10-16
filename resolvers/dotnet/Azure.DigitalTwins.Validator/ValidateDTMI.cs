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
    //TODO: remove partial classes. Move to single bool Validator.Validate that reads the file once, logs all the validation failures and returns true/false
    public static partial class Validations
    {

        public async static Task<bool> ValidateDTMI(this FileInfo fileInfo)
        {
            var fileText = await File.ReadAllTextAsync(fileInfo.FullName);
            return ValidateDTMI(fileText, fileInfo.FullName);
        }
        public static bool ValidateDTMI(string fileText, string fileName = "")
        {
            var dtmiRegex = new Regex("^dtmi:(?:_+[A-Za-z0-9]|[A-Za-z])(?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::(?:_+[A-Za-z0-9]|[A-Za-z])(?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
            var versionRegex = new Regex(";[1-9][0-9]{0,8}$");

            var model = JsonDocument.Parse(fileText).RootElement;
            JsonElement rootId;
            if (!model.TryGetProperty("@id", out rootId))
            {
                throw new MissingDTMIException(fileName);
            }

            var dtmiNamespace = versionRegex.Replace(rootId.GetString(), "");
            var exceptions = new List<Exception>();

            FindAllIds(fileText, (id) =>
            {
                if (!dtmiRegex.IsMatch(id))
                {
                    exceptions.Add(new InvalidDTMIException(id));
                    return false;
                }
                if (!id.StartsWith(dtmiNamespace))
                {
                    exceptions.Add(new InvalidSubDTMIException(id));
                    return false;
                }
                return true;
            }).ToList();

            exceptions.ForEach(ex =>
            {
                if (ex is InvalidDTMIException)
                    throw new InvalidDTMIException(ex.Message, ex);
                if (ex is InvalidSubDTMIException)
                    throw new InvalidSubDTMIException(ex.Message, ex);
            });
            return true;
        }
    }
}
