using System.Collections.Generic;
using System.Text.Json;

namespace Azure.Iot.ModelsRepository.CLI
{
    internal class FileExtractResult
    {
        public FileExtractResult(List<string> models, JsonValueKind kind)
        {
            Models = models;
            ContentKind = kind;
        }

        readonly public List<string> Models;

        // Used to validate cases such as a single model in an array.
        readonly public JsonValueKind ContentKind;
    }
}
