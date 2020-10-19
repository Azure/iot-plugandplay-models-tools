using System.Collections.Generic;
using System.Linq;

namespace Azure.DigitalTwins.Resolver
{
    public class ModelMetadata
    {
        public string Id { get; }
        public IList<string> Extends { get; }
        public IList<string> ComponentSchemas { get; }
        public IList<string> Dependencies { get { return Extends.Union(ComponentSchemas).ToList(); } }

        public ModelMetadata(string id, IList<string> extends, IList<string> componentSchemas)
        {
            this.Id = id;
            this.Extends = extends;
            this.ComponentSchemas = componentSchemas;
        }
    }
}
