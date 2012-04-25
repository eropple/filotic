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
    /// A file system that corresponds to a directory on disk. Is unusual in
    /// Filotic in that it _is_ initialized with a string-based path.
    /// </summary>
    /// <remarks>
    /// This is a fairly thin abstraction over the standard System.IO libraries;
    /// it translates calls into the expected System.IO.Directory and
    /// System.IO.File calls. Methods within can be expected to throw the
    /// exceptions that would normally be thrown by System.IO classes in the
    /// same situations.
    /// </remarks>
    public class DirectoryFileSystem : FileSystem
    {
        protected readonly String RootPath;
        protected readonly DirectoryPath RootPathObject = new DirectoryPath(new String[] {});

        public DirectoryFileSystem(String rootPath)
        {
            this.RootPath = rootPath;
        }

        public override DirectoryPath Root
        {
            get { return this.RootPathObject; }
        }

        public override bool Exists(FilePath path)
        {
            if (path.RawPathSegments.Length == 0) return false;

            return File.Exists(PathToRealPath(path));
        }

        public override bool Exists(DirectoryPath path)
        {
            if (path.RawPathSegments.Length == 0) return true;

            return Directory.Exists(PathToRealPath(path));
        }

        public override void Create(DirectoryPath directory)
        {
            Directory.CreateDirectory(PathToRealPath(directory));
        }

        public override Stream Create(FilePath file, bool openAsReadWrite = false, bool createParents = true)
        {
            String rawPath = PathToStringPath(file);
            if (File.Exists(rawPath)) throw new IOException(String.Format("File '{0}' already exists.", file));

            String parentPath = Path.GetDirectoryName(rawPath);
            if (createParents == true && Directory.Exists(parentPath) == false)
            {
                Directory.CreateDirectory(parentPath);
            }

            if (openAsReadWrite)
            {
                return File.Open(PathToRealPath(file), FileMode.Create, FileAccess.Write);
            }
            else
            {
                return File.Open(PathToRealPath(file), FileMode.Create, FileAccess.ReadWrite);
            }
        }

        public override void Delete(FilePath path)
        {
            File.Delete(PathToRealPath(path));
        }

        public override void Delete(DirectoryPath path, bool deleteChildPaths = false)
        {
            String rawPath = PathToRealPath(path);

            if (Directory.Exists(rawPath))
            {
                Directory.Delete(rawPath, deleteChildPaths);
            }
        }

        public override Stream Open(FilePath file, FileAccessMode accessMode)
        {
            switch (accessMode)
            {
                case FileAccessMode.Read:
                    return File.Open(PathToRealPath(file), FileMode.Open, FileAccess.Read);
                case FileAccessMode.Write:
                    return File.Open(PathToRealPath(file), FileMode.OpenOrCreate, FileAccess.Write);
                case FileAccessMode.ReadWrite:
                    return File.Open(PathToRealPath(file), FileMode.OpenOrCreate, FileAccess.ReadWrite);
                default:
                    throw new ArgumentException("Impossible value for accessMode.");
            }
        }

        public override ReadOnlyCollection<DirectoryPath> GetChildDirectories(DirectoryPath directory, Regex regex)
        {
            String rawPath = PathToStringPath(directory);

            String[] entries = Directory.GetDirectories(rawPath);
            List<DirectoryPath> output = new List<DirectoryPath>(entries.Length);
            foreach (String e in entries)
            {
                String filename = Path.GetFileName(e);

                if (regex.IsMatch(filename))
                {
                    output.Add(directory.AppendDirectory(filename));
                }
            }
            
            return new ReadOnlyCollection<DirectoryPath>(output);
        }

        public override ReadOnlyCollection<FilePath> GetChildFiles(DirectoryPath directory, Regex regex)
        {
            String rawPath = PathToStringPath(directory);

            String[] entries = Directory.GetFiles(rawPath);
            List<FilePath> output = new List<FilePath>(entries.Length);
            foreach (String e in entries)
            {
                String filename = Path.GetFileName(e);

                if (regex.IsMatch(filename))
                {
                    output.Add(directory.AppendFile(filename));
                }
            }

            return new ReadOnlyCollection<FilePath>(output);
        }

        public override ReadOnlyCollection<AbstractPath> GetChildPaths(DirectoryPath directory, Regex regex)
        {
            String rawPath = PathToStringPath(directory);

            String[] entries = Directory.GetFiles(rawPath);
            List<AbstractPath> output = new List<AbstractPath>(entries.Length);
            foreach (String e in entries)
            {
                String filename = Path.GetFileName(e);

                if (regex.IsMatch(filename))
                {
                    if (File.Exists(e))
                    {
                        output.Add(directory.AppendFile(filename));
                    }
                    else if (Directory.Exists(e))
                    {
                        output.Add(directory.AppendDirectory(filename));
                    }
                    else
                    {
                        throw new IOException("Invalid state: file system entry neither File nor Directory?s");
                    }
                }
            }

            return new ReadOnlyCollection<AbstractPath>(output);
        }

        protected String PathToStringPath(AbstractPath path)
        {
            String[] components = new String[path.RawPathSegments.Length + 1];
            components[0] = this.RootPath;
            path.RawPathSegments.CopyTo(components, 1);

            return System.IO.Path.Combine(components) + "/";
        }

        protected String PathToRealPath(AbstractPath path)
        {
            String rawPath = path.ToString();
            return Path.Combine(this.RootPath, rawPath.Substring(1));
        }

        public Boolean IsDisposed { get; private set; }
        public override void Dispose()
        {
            if (IsDisposed == false)
            {
                IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
