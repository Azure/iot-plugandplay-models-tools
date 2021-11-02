using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.IoT.ModelsRepository.ChangeCalc.Models
{
    class RepositoryUpdatesFormatted
    {
        internal string FilesAddedFormatted { get; set; }
        internal string FilesModifiedFormatted { get; set; }
        internal string FilesRemovedFormatted { get; set; }
        internal string FilesRenamedFormatted { get; set; }
        internal string FilesAddedModifiedFormatted { get; set; }
        internal string FilesAllFormatted { get; set; }
    }
}
