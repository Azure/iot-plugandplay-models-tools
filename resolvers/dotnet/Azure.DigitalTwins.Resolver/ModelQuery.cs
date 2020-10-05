﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

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
                    foreach (var element in contents.EnumerateArray())
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

        public class ModelMetadata {
            public string Id { get; }
            public IList<string> Extends { get; }
            public IList<string> ComponentSchemas { get; }
            public IList<string> Dependencies { get { return Extends.Union(ComponentSchemas).ToList();  } }

            public ModelMetadata(string id, IList<string> extends, IList<string> componentSchemas)
            {
                this.Id = id;
                this.Extends = extends;
                this.ComponentSchemas = componentSchemas;
            }
        }
    }
}
