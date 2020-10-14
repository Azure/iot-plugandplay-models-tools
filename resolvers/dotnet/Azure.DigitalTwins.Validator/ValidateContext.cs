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
        public async static Task<bool> ValidateContext(this FileInfo fileInfo)
        {
            var fileText = await File.ReadAllTextAsync(fileInfo.FullName);
            return ValidateContext(fileText, fileInfo.FullName);
        }
        public static bool ValidateContext(string fileText, string fileName = "")
        {
            var model = JsonDocument.Parse(fileText).RootElement;
            JsonElement rootId;
            if (!model.TryGetProperty("@context", out rootId))
            {
                throw new MissingContextException(fileName);
            }

            if (!rootId.GetString().Equals("dtmi:dtdl:context;2", StringComparison.InvariantCulture))
            {
                throw new InvalidContextException(fileName);
            }
            return true;
        }
    }
}