using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ed.Filotic.FileSystems;

namespace Ed.Filotic.Paths
{
    public sealed class FilePath : AbstractPath
    {
        public FilePath(String[] pathSegments) 
            : base(pathSegments)
        {
        }

        public override string ToString()
        {
            return "/" + String.Join("/", this.RawPathSegments);
        }
    }
}
