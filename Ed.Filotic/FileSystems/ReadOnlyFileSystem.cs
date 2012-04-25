using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ed.Filotic.Paths;

namespace Ed.Filotic.FileSystems
{
    /// <summary>
    /// A read-only adapter over another file system.
    /// </summary>
    /// <remarks>
    /// Attempting to create or delete files will throw UnauthorizedAccessException.
    /// Attempting to open a file in Write or ReadWrite modes will likewise result
    /// in an UnauthorizedAccessException.
    /// </remarks>
    public class ReadOnlyFileSystem : FileSystem
    {
        protected readonly FileSystem BaseFileSystem;

        public ReadOnlyFileSystem(FileSystem baseFileSystem)
        {
            this.BaseFileSystem = baseFileSystem;
        }


        public override DirectoryPath Root
        {
            get { return this.BaseFileSystem.Root; }
        }

        public override bool Exists(FilePath path)
        {
            return this.BaseFileSystem.Exists(path);
        }

        public override bool Exists(DirectoryPath path)
        {
            return this.BaseFileSystem.Exists(path);
        }

        public override void Create(DirectoryPath directory)
        {
            throw new UnauthorizedAccessException("Cannot modify files or directories through a ReadOnlyFileSystem.");
        }

        public override Stream Create(FilePath file, bool openAsReadWrite = false, bool createParents = true)
        {
            throw new UnauthorizedAccessException("Cannot modify files or directories through a ReadOnlyFileSystem.");
        }

        public override void Delete(DirectoryPath path, bool deleteChildPaths = false)
        {
            throw new UnauthorizedAccessException("Cannot modify files or directories through a ReadOnlyFileSystem.");
        }

        public override void Delete(FilePath path)
        {
            throw new UnauthorizedAccessException("Cannot modify files or directories through a ReadOnlyFileSystem.");
        }

        public override Stream Open(FilePath file, FileAccessMode accessMode)
        {
            if (accessMode == FileAccessMode.Read)
            {
                return this.BaseFileSystem.Open(file, accessMode);
            }
            
            throw new UnauthorizedAccessException("Cannot modify files or directories through a ReadOnlyFileSystem.");
        }

        public override ReadOnlyCollection<DirectoryPath> GetChildDirectories(DirectoryPath directory, Regex regex)
        {
            return this.BaseFileSystem.GetChildDirectories(directory, regex);
        }

        public override ReadOnlyCollection<FilePath> GetChildFiles(DirectoryPath directory, Regex regex)
        {
            return this.BaseFileSystem.GetChildFiles(directory, regex);
        }

        public override ReadOnlyCollection<AbstractPath> GetChildPaths(DirectoryPath directory, Regex regex)
        {
            return this.BaseFileSystem.GetChildPaths(directory, regex);
        }

        public override void Dispose()
        {
            this.BaseFileSystem.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
