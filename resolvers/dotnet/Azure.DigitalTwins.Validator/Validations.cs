using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.DigitalTwins.Validator.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Azure.DigitalTwins.Validator
{
    public static class Validations
    {
        public async static Task<bool> Validate(this FileInfo fileInfo, ILogger logger = null)
        {
            var fileName = fileInfo.FullName;
            var fileText = await File.ReadAllTextAsync(fileName);
            var model = JsonDocument.Parse(fileText).RootElement;


            return ValidateFilePath(fileName, logger) &
                ScanForReservedWords(fileText, logger) &
                ValidateContext(model, fileName, logger) &
                ValidateDTMIs(model,fileName, logger);
        }
        public static bool FindAllIds(string fileText, Func<string, bool> validation)
        {
            var valid = true;
            var idRegex = new Regex("\\\"@id\\\":\\s?\\\"[^\\\"]*\\\",?");
            foreach (Match id in idRegex.Matches(fileText))
            {
                // return just the value without "@id" and quotes
                var idValue = Regex.Replace(Regex.Replace(id.Value, "\\\"@id\\\":\\s?\"", ""), "\",?", "");
                valid = valid & validation(idValue);
            }

            return valid;
        }

        public static bool ValidateFilePath(string fullPath, ILogger logger = null)
        {
            logger = logger ?? NullLogger.Instance;
            var filePathRegex = new Regex("dtmi[\\\\\\/](?:_+[a-z0-9]|[a-z])(?:[a-z0-9_]*[a-z0-9])?(?:[\\\\\\/](?:_+[a-z0-9]|[a-z])(?:[a-z0-9_]*[a-z0-9])?)*-[1-9][0-9]{0,8}\\.json$");

            if (!filePathRegex.IsMatch(fullPath))
            {
                logger.LogError( $"File '{fullPath}' does not adhere to naming rules.");
                return false;
            }
            return true;
        }

         public static bool ScanForReservedWords(string fileText, ILogger logger = null)
        {
            logger = logger ?? NullLogger.Instance;

            var reservedRegEx = new Regex("Microsoft|Azure", RegexOptions.IgnoreCase);
            return FindAllIds(fileText, (id) =>
            {
                if (reservedRegEx.IsMatch(id))
                {
                    logger.LogError($"Reserved words found in the following:\n{string.Join(",\n", id)}");
                    return false;
                }
                return true;
            });
        }

        public static bool ValidateContext(JsonElement model, string fileName = "", ILogger logger = null)
        {
            logger = logger ?? NullLogger.Instance;
            JsonElement contextElement;
            if (!model.TryGetProperty("@context", out contextElement))
            {
                logger.LogError("File '{fileName}' does not have a root \"@context\" element");
                return false;
            }

            if (!contextElement.GetString().Equals("dtmi:dtdl:context;2", StringComparison.InvariantCulture))
            {
                logger.LogError($"File '{fileName}' has an invalid \"@context\" element");
                return false;
            }
            return true;
        }
        public static bool ValidateDTMIs(JsonElement model, string fileName = "", ILogger logger = null)
        {
            logger = logger ?? NullLogger.Instance;
            var dtmiNamespace = GetDtmiNamespace(GetRootId(model, fileName));
            var fileText = model.ToString();
            try
            {
                return FindAllIds(fileText, (id) =>
                {
                    if (!IsDtmi(id))
                    {
                        logger.LogError($"Invalid DTMI format:\n{id}");
                        return false;
                    }
                    if (!id.StartsWith(dtmiNamespace))
                    {
                        logger.LogError($"Invalid sub DTMI format:\n{id}");
                        return false;
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return false;
            }
        }

        public static JsonElement GetRootId(JsonElement model, string fileName)
        {
            JsonElement rootId;
            if (!model.TryGetProperty("@id", out rootId))
            {
                throw new MissingDTMIException(fileName);
            }

            return rootId;
        }

        public static bool IsDtmi(string id)
        {
            var dtmiRegex = new Regex("^dtmi:(?:_+[A-Za-z0-9]|[A-Za-z])(?:[A-Za-z0-9_]*[A-Za-z0-9])?(?::(?:_+[A-Za-z0-9]|[A-Za-z])(?:[A-Za-z0-9_]*[A-Za-z0-9])?)*;[1-9][0-9]{0,8}$");
            return dtmiRegex.IsMatch(id);
        }

        public static string GetDtmiNamespace(JsonElement id)
        {
            var versionRegex = new Regex(";[1-9][0-9]{0,8}$");
            return versionRegex.Replace(id.GetString(), "");
        }
    }
}