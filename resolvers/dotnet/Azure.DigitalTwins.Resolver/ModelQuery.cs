using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azure.DigitalTwins.Resolver
{
    public class ModelQuery
    {
        readonly string _content;
        readonly JsonDocumentOptions _parseOptions;

        public ModelQuery(string content)
        {
            _content = content;
            _parseOptions = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };
        }

        public ModelMetadata GetMetadata()
        {
            return new ModelMetadata(GetId(), GetExtends(), GetComponentSchemas());
        }

        public string GetId()
        {
            using JsonDocument document = JsonDocument.Parse(_content, _parseOptions);
            JsonElement _root = document.RootElement;

            if (_root.TryGetProperty("@id", out JsonElement id))
            {
                if (id.ValueKind == JsonValueKind.String)
                {
                    return id.GetString();
                }
            }

            return string.Empty;
        }

        public IList<string> GetExtends()
        {
            using JsonDocument document = JsonDocument.Parse(_content, _parseOptions);
            JsonElement _root = document.RootElement;

            List<string> dependencies = new List<string>();

            if (_root.TryGetProperty("extends", out JsonElement extends))
            {
                if (extends.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement extendElement in extends.EnumerateArray())
                    {
                        if (extendElement.ValueKind == JsonValueKind.String)
                        {
                            dependencies.Add(extendElement.GetString());
                        }
                    }
                }
                else if (extends.ValueKind == JsonValueKind.String)
                {
                    dependencies.Add(extends.GetString());
                }
            }

            return dependencies;
        }

        public IList<string> GetComponentSchemas()
        {
            using JsonDocument document = JsonDocument.Parse(_content, _parseOptions);
            JsonElement _root = document.RootElement;

            List<string> componentSchemas = new List<string>();

            if (_root.TryGetProperty("contents", out JsonElement contents))
            {
                if (contents.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement element in contents.EnumerateArray())
                    {
                        if (element.TryGetProperty("@type", out JsonElement type))
                        {
                            if (type.ValueKind == JsonValueKind.String && type.GetString() == "Component")
                            {
                                if (element.TryGetProperty("schema", out JsonElement schema))
                                {
                                    if (schema.ValueKind == JsonValueKind.String)
                                    {
                                        componentSchemas.Add(schema.GetString());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return componentSchemas;
        }

        public async Task<Dictionary<string, string>> ListToDictAsync()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            using JsonDocument document = JsonDocument.Parse(_content, _parseOptions);
            JsonElement _root = document.RootElement;

            if (_root.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement element in _root.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.Object)
                    {
                        using MemoryStream stream = new MemoryStream();
                        await JsonSerializer.SerializeAsync(stream, element);
                        stream.Position = 0;

                        using StreamReader streamReader = new StreamReader(stream);
                        string serialized = await streamReader.ReadToEndAsync();

                        string id = new ModelQuery(serialized).GetId();
                        result.Add(id, serialized);
                    }
                }
            }

            return result;
        }
    }
}
