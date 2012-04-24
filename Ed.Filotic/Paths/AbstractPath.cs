using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Ed.Filotic.FileSystems;

namespace Ed.Filotic.Paths
{
    /// <summary>
    /// The base interface for all file system paths.
    /// </summary>
    /// <remarks>
    /// The options available when you're handing around IPaths are kept
    /// intentionally fairly meager, as Filotic expects you to differentiate
    /// at an object level between directories and files.
    /// </remarks>
    public abstract class AbstractPath
    {
        /// <summary>
        /// The strings that make up a path within the file system. ToString
        /// returns these, prepended and joined by /, with a / appended for all
        /// directories.
        /// </summary>
        public readonly ReadOnlyCollection<String> PathSegments;
        /// <summary>
        /// The strings that make up a path within the file system. Included in
        /// AbstractPath to allow for faster internal operations; should never,
        /// under any circumstances, be modified.
        /// </summary>
        internal readonly String[] RawPathSegments;

        protected AbstractPath(String[] pathSegments)
        {
            for (Int32 i = 0; i < pathSegments.Length; ++i)
            {
                pathSegments[i] = pathSegments[i].Trim();
                if (pathSegments[i] == String.Empty)
                {
                    throw new IOException("Path segments may not be all-whitespace or the empty string.");
                }
            }

            this.RawPathSegments = pathSegments;
            this.PathSegments = new ReadOnlyCollection<String>(pathSegments);
        }

        /// <summary>
        /// Whether or not this path is the root path of the current file system.
        /// </summary>
        public Boolean IsRoot { get { return RawPathSegments.Length == 0; } }

        public Int32 SegmentCount { get { return RawPathSegments.Length; } }

        private DirectoryPath _parent = null;
        public DirectoryPath Parent
        {
            get
            {
                if (_parent == null)
                {
                    if (this.IsRoot)
                    {
                        throw new InvalidOperationException("The root node has no parent.");
                    }

                    String[] parentSegments = new String[RawPathSegments.Length - 1];
                    Array.Copy(RawPathSegments, parentSegments, RawPathSegments.Length - 1);
                    _parent = new DirectoryPath(parentSegments);
                }
                return _parent;
            }
        }
    }
}
