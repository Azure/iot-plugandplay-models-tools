using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Microsoft.IoT.ModelsRepository.CommandLine
{
    internal class ModelIndexEntry
    {
        [JsonIgnore]
        public string Dtmi { get; set; }
        [JsonPropertyName("displayName")]
        public object DisplayName { get; set; }
        [JsonPropertyName("description")]
        public object Description { get; set; }
    }

    internal class ModelIndex : Dictionary<string, ModelIndexEntry>
    {
    }
}
