using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ed.Filotic.FileSystems
{
    public sealed class FileSystemHelper
    {
        private readonly FileSystem parentFileSystem;

        internal FileSystemHelper(FileSystem fs)
        {
            this.parentFileSystem = fs;
        }
    }
}
