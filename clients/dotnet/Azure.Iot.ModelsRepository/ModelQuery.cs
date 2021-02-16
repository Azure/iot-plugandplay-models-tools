using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Azure.Iot.ModelsRepository
{
    internal class ModelQuery
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

        public ModelMetadata ParseModel()
        {
            using (JsonDocument document = JsonDocument.Parse(_content, _parseOptions))
            {
                return ParseInterface(document.RootElement);
            }
        }

        private ModelMetadata ParseInterface(JsonElement root)
        {
            string rootDtmi = ParseRootDtmi(root);
            IList<string> extends = ParseExtends(root);
            IList<string> contents = ParseContents(root);

            return new ModelMetadata(rootDtmi, extends, contents);
        }

        private string ParseRootDtmi(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(ModelProperties.Dtmi, out JsonElement id))
            {
                if (id.ValueKind == JsonValueKind.String)
                {
                    return id.GetString();
                }
            }

            return string.Empty;
        }

        private IList<string> ParseExtends(JsonElement root)
        {
            List<string> dependencies = new List<string>();

            if (root.ValueKind != JsonValueKind.Object)
            {
                return dependencies;
            }

            if (!root.TryGetProperty(ModelProperties.Extends, out JsonElement extends))
            {
                return dependencies;
            }

            if (extends.ValueKind == JsonValueKind.String)
            {
                dependencies.Add(extends.GetString());
            }

            else if (extends.ValueKind == JsonValueKind.Object &&
                extends.TryGetProperty(ModelProperties.Type, out JsonElement objectType) &&
                objectType.ValueKind == JsonValueKind.String &&
                objectType.GetString() == ModelProperties.TypeValueInterface)
            {
                ModelMetadata meta = ParseInterface(extends);
                dependencies.AddRange(meta.Dependencies);
            }

            else if (extends.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement extendElement in extends.EnumerateArray())
                {
                    if (extendElement.ValueKind == JsonValueKind.String)
                    {
                        dependencies.Add(extendElement.GetString());
                    }
                    // Extends can have multiple levels and contain inline interfaces.
                    else if (extendElement.ValueKind == JsonValueKind.Object &&
                        extendElement.TryGetProperty(ModelProperties.Type, out JsonElement elementObjectType) &&
                        elementObjectType.ValueKind == JsonValueKind.String &&
                        elementObjectType.GetString() == ModelProperties.TypeValueInterface)
                    {
                        ModelMetadata meta = ParseInterface(extendElement);
                        dependencies.AddRange(meta.Dependencies);
                    }
                }
            }

            return dependencies;
        }

        private IList<string> ParseContents(JsonElement root)
        {
            List<string> dependencies = new List<string>();

            if (root.ValueKind != JsonValueKind.Object)
            {
                return dependencies;
            }

            if (!root.TryGetProperty(ModelProperties.Contents, out JsonElement contents))
            {
                return dependencies;
            }

            if (contents.ValueKind != JsonValueKind.Array)
            {
                return dependencies;
            }

            foreach (JsonElement contentElement in contents.EnumerateArray())
            {
                if (contentElement.TryGetProperty(ModelProperties.Type, out JsonElement contentElementType) &&
                    contentElementType.ValueKind == JsonValueKind.String &&
                    contentElementType.GetString() == ModelProperties.TypeValueComponent)
                {
                    dependencies.AddRange(ParseComponent(contentElement));
                }
            }

            return dependencies;
        }

        private IList<string> ParseComponent(JsonElement root)
        {
            // We already know root is an object of @type Component

            List<string> dependencies = new List<string>();

            if (!root.TryGetProperty(ModelProperties.Schema, out JsonElement componentSchema))
            {
                return dependencies;
            }

            if (componentSchema.ValueKind == JsonValueKind.String)
            {
                dependencies.Add(componentSchema.GetString());
            }
            else if (componentSchema.ValueKind == JsonValueKind.Object)
            {
                if (componentSchema.TryGetProperty(ModelProperties.Type, out JsonElement componentSchemaType) &&
                    componentSchemaType.ValueKind == JsonValueKind.String &&
                    componentSchemaType.GetString() == ModelProperties.TypeValueInterface)
                {
                    ModelMetadata meta = ParseInterface(componentSchema);
                    dependencies.AddRange(meta.Dependencies);
                }
            }
            else if (componentSchema.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement componentSchemaElement in componentSchema.EnumerateArray())
                {
                    if (componentSchemaElement.ValueKind == JsonValueKind.String)
                    {
                        dependencies.Add(componentSchemaElement.GetString());
                    }
                    else if (componentSchemaElement.ValueKind == JsonValueKind.Object)
                    {
                        if (componentSchemaElement.TryGetProperty(ModelProperties.Type, out JsonElement componentSchemaElementType) &&
                            componentSchemaElementType.ValueKind == JsonValueKind.String &&
                            componentSchemaElementType.GetString() == ModelProperties.TypeValueInterface)
                        {
                            ModelMetadata meta = ParseInterface(componentSchemaElement);
                            dependencies.AddRange(meta.Dependencies);
                        }
                    }
                }
            }

            return dependencies;
        }

        public async Task<Dictionary<string, string>> ListToDictAsync()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            using (JsonDocument document = JsonDocument.Parse(_content, _parseOptions))
            {
                JsonElement _root = document.RootElement;

                if (_root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement element in _root.EnumerateArray())
                    {
                        if (element.ValueKind == JsonValueKind.Object)
                        {
                            using (MemoryStream stream = new MemoryStream())
                            {
                                await JsonSerializer.SerializeAsync(stream, element);
                                stream.Position = 0;

                                using (StreamReader streamReader = new StreamReader(stream))
                                {
                                    string serialized = await streamReader.ReadToEndAsync();

                                    string id = new ModelQuery(serialized).ParseModel().Id;
                                    result.Add(id, serialized);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
