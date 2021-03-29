using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoT.ModelValidator.Models
{
    public class RepositoryUpdatesFormatted
    {
        public string FilesAddedFormatted { get; set; }
        public string FilesModifiedFormatted { get; set; }
        public string FilesRemovedFormatted { get; set; }
        public string FilesRenamedFormatted { get; set; }
        public string FilesAddedModifiedFormatted { get; set; }
        public string FilesAllFormatted { get; set; }
    }
}
