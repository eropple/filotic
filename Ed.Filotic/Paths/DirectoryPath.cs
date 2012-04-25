using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ed.Filotic.FileSystems;

namespace Ed.Filotic.Paths
{
    /// <summary>
    /// A representation of a directory within a file system.
    /// </summary>
    public class DirectoryPath : AbstractPath
    {
        public DirectoryPath(String[] pathSegments) 
            : base(pathSegments)
        {
        }

        /// <summary>
        /// Creates a DirectoryPath object pointing to a child of this directory.
        /// If a segment is given with slashes ("/") within it, these will be
        /// broken up into sub-segments.
        /// </summary>
        /// <remarks>
        /// Note that being able to create this DirectoryPath does not mean that 
        /// the DirectoryPath exists or can be created.
        /// </remarks>
        /// <param name="segment">The path segment to append.</param>
        /// <returns>
        /// a DirectoryPath object that corresponds to the requested path.
        /// </returns>
        public DirectoryPath AppendDirectory(String segment) { return this.AppendDirectory(segment.Split('/')); }

        /// <summary>
        /// Creates a DirectoryPath object that corresponds to the given path,
        /// as a child of this directory.
        /// </summary>
        /// <remarks>
        /// Note that being able to create this DirectoryPath does not mean that 
        /// the DirectoryPath exists or can be created.
        /// </remarks>
        /// <param name="segments">The path segments to append.</param>
        /// <returns>
        /// a DirectoryPath object that corresponds to the requested path.
        /// </returns>
        public DirectoryPath AppendDirectory(params String[] segments)
        {
            String[] newSegments = new String[this.RawPathSegments.Length + segments.Length];
            this.RawPathSegments.CopyTo(newSegments, 0);
            segments.CopyTo(newSegments, this.RawPathSegments.Length);

            return new DirectoryPath(newSegments);
        }

        /// <summary>
        /// Creates a FilePath object pointing to a child of this directory.
        /// If a segment is given with slashes ("/") within it, these will be
        /// broken up into sub-segments.
        /// </summary>
        /// <remarks>
        /// Note that being able to create this FilePath does not mean that 
        /// the FilePath exists or can be created.
        /// </remarks>
        /// <param name="segment">The path segment to append.</param>
        /// <returns>
        /// a FilePath object that corresponds to the requested path.
        /// </returns>
        public FilePath AppendFile(String segment) { return this.AppendFile(segment.Split('/')); }

        /// <summary>
        /// Creates a FilePath object pointing to a child of this directory.
        /// </summary>
        /// <remarks>
        /// Note that being able to create this FilePath does not mean that 
        /// the FilePath exists or can be created.
        /// </remarks>
        /// <param name="segments">The path segments to append.</param>
        /// <returns>
        /// a FilePath object that corresponds to the requested path.
        /// </returns>
        public FilePath AppendFile(params String[] segments)
        {
            String[] newSegments = new String[this.RawPathSegments.Length + segments.Length];
            this.RawPathSegments.CopyTo(newSegments, 0);
            segments.CopyTo(newSegments, this.RawPathSegments.Length);

            return new FilePath(newSegments);
        }

        public override string ToString()
        {
            return ((RawPathSegments.Length > 0) ? "/" : "") + String.Join("/", this.RawPathSegments) + "/";
        }
    }
}
