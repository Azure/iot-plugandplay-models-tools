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

    internal class ModelIndexLinks
    {
        [JsonPropertyName("next")]
        public string Next { get; set; }
        [JsonPropertyName("prev")]
        public string Prev { get; set; }
        [JsonPropertyName("self")]
        public string Self { get; set; }
    }

    internal class ModelDictionary : Dictionary<string, ModelIndexEntry>
    {
    }

    internal class ModelIndex
    {
        public ModelIndex(ModelDictionary models, ModelIndexLinks links = null)
        {
            Links = links;
            Models = models;
            Version = "1.0";
        }

        [JsonPropertyName("links")]
        public ModelIndexLinks Links { get; }
        [JsonPropertyName("models")]
        public ModelDictionary Models { get; }
        [JsonPropertyName("version")]
        public string Version { get; }
    }
}
