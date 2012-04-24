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
    /// A file system that sits as a descendant of a given parent file system,
    /// with its root at a given path of the parent file system.
    /// </summary>
    /// <remarks>
    /// Useful for concealing the majority of a file system in use cases where
    /// you need to prevent a user from moving out of their workspace and into
    /// restricted directories.
    /// 
    /// It should be noted that this isn't even close to being as secure as a
    /// chroot jail or even a subst'ed volume; if, for example, reflection-based
    /// code is in use, it's trivial to get the parent file system out of this
    /// class and make calls directly through it. It should be reasonably secure
    /// for use in low-trust environments where reflection isn't available, but
    /// don't stake your life or your company on it.
    /// </remarks>
    public class SubtreeFileSystem : FileSystem
    {
        protected readonly FileSystem BaseFileSystem;
        protected readonly DirectoryPath BaseFileSystemPath;

        protected readonly DirectoryPath LocalRoot = new DirectoryPath(new String[] {});

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="baseFileSystem">The file system to which this is an adjunct.</param>
        /// <param name="baseFileSystemPath">The file system path </param>
        public SubtreeFileSystem(FileSystem baseFileSystem, DirectoryPath baseFileSystemPath)
        {
            if (baseFileSystem.Exists(baseFileSystemPath) == false)
            {
                throw new IOException(String.Format("Cannot instantiate a SubtreeFileSystem upon a " +
                    "path that doesn't exist on its parent: '{0}'", baseFileSystemPath));
            }

            BaseFileSystem = baseFileSystem;
            BaseFileSystemPath = baseFileSystemPath;
        }

        public override DirectoryPath Root
        {
            get { return LocalRoot; }
        }

        public override bool Exists(FilePath path)
        {
            return this.BaseFileSystem.Exists(TranslatePathToBaseSystem(path));
        }

        public override bool Exists(DirectoryPath path)
        {
            return this.BaseFileSystem.Exists(TranslatePathToBaseSystem(path));
        }

        public override void Create(DirectoryPath directory)
        {
            this.BaseFileSystem.Create(TranslatePathToBaseSystem(directory));
        }

        public override Stream Create(FilePath file, bool openAsReadWrite = false, bool createParents = true)
        {
            return this.BaseFileSystem.Create(TranslatePathToBaseSystem(file), openAsReadWrite, createParents);
        }

        public override void Delete(FilePath path)
        {
            this.BaseFileSystem.Delete(TranslatePathToBaseSystem(path));
        }

        public override void Delete(DirectoryPath path, bool deleteChildPaths = false)
        {
            this.BaseFileSystem.Delete(TranslatePathToBaseSystem(path), deleteChildPaths);
        }

        public override Stream Open(FilePath file, FileAccessMode accessMode)
        {
            return this.BaseFileSystem.Open(TranslatePathToBaseSystem(file), accessMode);
        }

        public override ReadOnlyCollection<DirectoryPath> GetChildDirectories(DirectoryPath directory, Regex regex)
        {
            var entries = this.BaseFileSystem.GetChildDirectories(TranslatePathToBaseSystem(directory), regex);
            List<DirectoryPath> translatedList = new List<DirectoryPath>(entries.Count);
            foreach (DirectoryPath e in entries)
            {
                translatedList.Add(TranslatePathFromBaseSystem(e));
            }
            return new ReadOnlyCollection<DirectoryPath>(translatedList);
        }

        public override ReadOnlyCollection<FilePath> GetChildFiles(DirectoryPath directory, Regex regex)
        {
            var entries = this.BaseFileSystem.GetChildFiles(TranslatePathToBaseSystem(directory), regex);
            List<FilePath> translatedList = new List<FilePath>(entries.Count);
            foreach (FilePath e in entries)
            {
                translatedList.Add(TranslatePathFromBaseSystem(e));
            }
            return new ReadOnlyCollection<FilePath>(translatedList);
        }

        public override ReadOnlyCollection<AbstractPath> GetChildPaths(DirectoryPath directory, Regex regex)
        {
            var entries = this.BaseFileSystem.GetChildPaths(TranslatePathToBaseSystem(directory), regex);
            List<AbstractPath> translatedList = new List<AbstractPath>(entries.Count);
            foreach (AbstractPath e in entries)
            {
                translatedList.Add(TranslatePathFromBaseSystem(e));
            }
            return new ReadOnlyCollection<AbstractPath>(translatedList);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        private DirectoryPath TranslatePathToBaseSystem(DirectoryPath path)
        {
            return this.BaseFileSystemPath.AppendDirectory(path.RawPathSegments);
        }

        private FilePath TranslatePathToBaseSystem(FilePath path)
        {
            return this.BaseFileSystemPath.AppendFile(path.RawPathSegments);
        }


        // there's probably a cleaner way to do this that's better for performance;
        // suggestions welcome!

        private DirectoryPath TranslatePathFromBaseSystem(DirectoryPath path)
        {
            Int32 length = path.RawPathSegments.Length - BaseFileSystemPath.RawPathSegments.Length;
            String[] newSegments = new String[length];
            Array.Copy(path.RawPathSegments, BaseFileSystemPath.RawPathSegments.Length, newSegments, 0, length);
            return new DirectoryPath(newSegments);
        }

        private FilePath TranslatePathFromBaseSystem(FilePath path)
        {
            Int32 length = path.RawPathSegments.Length - BaseFileSystemPath.RawPathSegments.Length;
            String[] newSegments = new String[length];
            Array.Copy(path.RawPathSegments, BaseFileSystemPath.RawPathSegments.Length, newSegments, 0, length);
            return new FilePath(newSegments);
        }

        private AbstractPath TranslatePathFromBaseSystem(AbstractPath path)
        {
            if (path is DirectoryPath)
                return TranslatePathFromBaseSystem((DirectoryPath)path);
            else
                return TranslatePathFromBaseSystem((FilePath) path);
        }
    }
}
