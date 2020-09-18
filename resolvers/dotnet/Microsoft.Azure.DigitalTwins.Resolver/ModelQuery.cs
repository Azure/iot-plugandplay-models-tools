using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Azure.DigitalTwins.Resolver
{
    public class ModelQuery
    {
        public List<string> GetDependencies(string modelContent)
        {
            List<string> result = new List<string>();

            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };

            using JsonDocument document = JsonDocument.Parse(modelContent, options);
            JsonElement root = document.RootElement;

            if (root.TryGetProperty("extends", out JsonElement extends))
            {
                if (extends.ValueKind == JsonValueKind.Array)
                {
                    foreach(var element in extends.EnumerateArray())
                    {
                        result.Add(element.GetString());
                    }
                }
                else
                {
                    result.Add(extends.GetString());
                }
            }

            if (root.TryGetProperty("contents", out JsonElement contents))
            {
                List<JsonElement> components = new List<JsonElement>();

                foreach(var element in contents.EnumerateArray())
                {
                    if (element.TryGetProperty("@type", out JsonElement type))
                    {
                        if (type.ValueKind == JsonValueKind.String && type.GetString() == "Component")
                        {
                            components.Add(element);
                        }
                    }
                }

                // TODO: Refactor
                foreach(var component in components)
                {
                    if (component.TryGetProperty("schema", out JsonElement schema))
                    {
                        if (schema.ValueKind == JsonValueKind.String)
                        {
                            string schemaValue = schema.GetString();
                            if (!result.Contains(schemaValue))
                            {
                                result.Add(schemaValue);
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
