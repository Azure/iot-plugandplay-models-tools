using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoT.ModelValidator.Models
{
    public class RepositoryUpdates
    {
        public IList<string> FilesAdded { get; set; }
        public IList<string> FilesModified { get; set; }
        public IList<string> FilesRemoved { get; set; }
        public IList<string> FilesRenamed { get; set; }
        public IList<string> FilesAddedModified { get; set; }
        public IList<string> FilesAll { get; set; }
    }
}
