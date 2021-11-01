using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoT.ModelsRepository.ChangeCalc.Models
{
    class RepositoryUpdates
    {
        internal IList<string> FilesAdded { get; set; }
        internal IList<string> FilesModified { get; set; }
        internal IList<string> FilesRemoved { get; set; }
        internal IList<string> FilesRenamed { get; set; }
        internal IList<string> FilesAddedModified { get; set; }
        internal IList<string> FilesAll { get; set; }
    }
}
