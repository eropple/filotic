using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Ed.Filotic.Paths;

namespace Ed.Filotic.FileSystems
{
    /// <summary>
    /// A file system that serves as a virtual directory of child file systems.
    /// </summary>
    /// <remarks>
    /// At present, MountPointFileSystem is a single-layer file system, after
    /// which it hands off to a child file system. It probably wouldn't be hard
    /// to extend this file system to allow for multiple layers of directories
    /// and mount points as leafs (in fact, I'll probably end up writing most
    /// of it when I write MemoryFileSystem), but for now this is all I needed
    /// for my own projects - specifically, to implement PhysicalFileSystem on
    /// Windows.
    /// 
    /// If you'd like to extend MountPointFileSystem in that direction,
    /// contributions are very welcome.
    /// </remarks>
    public class MountPointFileSystem : FileSystem
    {
        protected internal readonly Dictionary<String, FileSystem> Mounts;
        protected internal readonly Dictionary<String, DirectoryPath> MountPaths;
        protected internal readonly DirectoryPath RootPath = new DirectoryPath(new String[] {});

        private readonly ReadOnlyCollection<FilePath> _emptyFileCollection =
            new ReadOnlyCollection<FilePath>(new List<FilePath>());

        public MountPointFileSystem()
        {
            Mounts = new Dictionary<String, FileSystem>();
            MountPaths = new Dictionary<String, DirectoryPath>();
        }

        /// <summary>
        /// Registers a file system to a mount point on this file system.
        /// </summary>
        /// <param name="fileSystem">
        /// A file system to mount to this one.
        /// </param>
        /// <param name="mountPath">
        /// The first-level path in which to mount this file system.
        /// </param>
        public virtual void Mount(FileSystem fileSystem, String mountPath)
        {
            mountPath = mountPath.Trim();

            if (mountPath == "." || mountPath.Contains("..") || mountPath.Contains("/"))
            {
                throw new ArgumentException("Mount path cannot be '.' or contain " +
                    "'..' or '/'.");
            }

            if (Mounts.ContainsKey(mountPath))
            {
                throw new ArgumentException("A file system is already mounted for '" + 
                    mountPath + "'.");
            }

            Mounts.Add(mountPath, fileSystem);
            MountPaths.Add(mountPath, new DirectoryPath(new String[] { mountPath }));
        }

        /// <summary>
        /// Unregisters a path from this file system.
        /// </summary>
        /// <remarks>
        /// If the path was not registered, nothing happens.
        /// </remarks>
        /// <param name="mountPath"></param>
        public virtual void Unmount(String mountPath)
        {
            String s = mountPath.Trim();
            Mounts.Remove(s);
            MountPaths.Remove(s);
        }

        /// <summary>
        /// Unmounts all mounted file systems.
        /// </summary>
        public virtual void UnmountAll()
        {
            Mounts.Clear();
            MountPaths.Clear();
        }

        public override DirectoryPath Root
        {
            get { return this.RootPath; }
        }

        public override bool Exists(FilePath path)
        {
            if (path.RawPathSegments.Length < 2)
            {
                return false; // no files at the root level
            }

            if (Mounts.ContainsKey(path.RawPathSegments[0]) == false) return false;

            FilePath sub;
            FileSystem fs = TransformPathToMountedPath(path, out sub);
            return fs.Exists(sub);
        }

        public override bool Exists(DirectoryPath path)
        {
            switch (path.RawPathSegments.Length)
            {
                case 0:
                    return true;
                case 1:
                    return Mounts.ContainsKey(path.RawPathSegments[0]);
                default:
                    if (Mounts.ContainsKey(path.RawPathSegments[0]) == false) return false;

                    DirectoryPath sub;
                    FileSystem fs = TransformPathToMountedPath(path, out sub);
                    return fs.Exists(sub);
            }
        }

        public override void Create(DirectoryPath directory)
        {
            if (directory.RawPathSegments.Length < 2)
            {
                throw new InvalidOperationException("The root and mount point levels " +
                    "may not have files or directories created within them.");
            }
            DirectoryPath sub;
            FileSystem fs = TransformPathToMountedPath(directory, out sub);
            fs.Create(sub);
        }

        public override Stream Create(FilePath file, bool openAsReadWrite = false, bool createParents = true)
        {
            if (file.RawPathSegments.Length < 2)
            {
                throw new InvalidOperationException("The root and mount point levels " +
                    "may not have files or directories created within them.");
            }
            FilePath sub;
            FileSystem fs = TransformPathToMountedPath(file, out sub);
            return fs.Create(sub, openAsReadWrite, createParents);
        }

        public override void Delete(DirectoryPath path, bool deleteChildPaths = false)
        {
            if (path.RawPathSegments.Length < 2)
            {
                throw new InvalidOperationException("The root and mount points cannot " +
                    "be deleted. Use Unmount() instead.");
            }
            DirectoryPath sub;
            FileSystem fs = TransformPathToMountedPath(path, out sub);
            fs.Delete(sub, deleteChildPaths);
        }

        public override void Delete(FilePath path)
        {
            if (path.RawPathSegments.Length < 2)
            {
                throw new InvalidOperationException("The root and mount points cannot " +
                    "be deleted. Use Unmount() instead.");
            }
            FilePath sub;
            FileSystem fs = TransformPathToMountedPath(path, out sub);
            fs.Delete(sub);
        }

        public override Stream Open(FilePath file, FileAccessMode accessMode)
        {
            if (file.RawPathSegments.Length < 2)
            {
                throw new InvalidOperationException("The root and mount points cannot " +
                    "be opened.");
            }
            FilePath sub;
            FileSystem fs = TransformPathToMountedPath(file, out sub);
            return fs.Open(sub, accessMode);
        }

        public override ReadOnlyCollection<DirectoryPath> GetChildDirectories(DirectoryPath directory, Regex regex)
        {
            if (directory.RawPathSegments.Length == 0)
            {
                List<DirectoryPath> d = new List<DirectoryPath>(Mounts.Count);
                d.AddRange(MountPaths.Values);
                return new ReadOnlyCollection<DirectoryPath>(d);
            }

            DirectoryPath sub;
            FileSystem fs = TransformPathToMountedPath(directory, out sub);
            ReadOnlyCollection<DirectoryPath> paths = fs.GetChildDirectories(sub, regex);
            List<DirectoryPath> newPaths = new List<DirectoryPath>(paths.Count);
            foreach (DirectoryPath path in paths)
            {
                String[] newSegments = new String[path.RawPathSegments.Length + 1];
                newSegments[0] = directory.RawPathSegments[0];
                Array.Copy(path.RawPathSegments, 0, newSegments, 1, path.RawPathSegments.Length);
                newPaths.Add(new DirectoryPath(newSegments));
            }

            return new ReadOnlyCollection<DirectoryPath>(newPaths);
        }

        public override ReadOnlyCollection<FilePath> GetChildFiles(DirectoryPath directory, Regex regex)
        {
            if (directory.RawPathSegments.Length == 0)
            {
                return _emptyFileCollection;
            }

            DirectoryPath sub;
            FileSystem fs = TransformPathToMountedPath(directory, out sub);
            ReadOnlyCollection<FilePath> paths = fs.GetChildFiles(sub, regex);
            List<FilePath> newPaths = new List<FilePath>(paths.Count);
            foreach (FilePath path in paths)
            {
                String[] newSegments = new String[path.RawPathSegments.Length + 1];
                newSegments[0] = directory.RawPathSegments[0];
                Array.Copy(path.RawPathSegments, 0, newSegments, 1, path.RawPathSegments.Length);
                newPaths.Add(new FilePath(newSegments));
            }

            return new ReadOnlyCollection<FilePath>(newPaths);
        }

        public override ReadOnlyCollection<AbstractPath> GetChildPaths(DirectoryPath directory, Regex regex)
        {
            if (directory.RawPathSegments.Length == 0)
            {
                List<AbstractPath> a = new List<AbstractPath>(Mounts.Count);
                a.AddRange(MountPaths.Values);
                return new ReadOnlyCollection<AbstractPath>(a);
            }

            DirectoryPath sub;
            FileSystem fs = TransformPathToMountedPath(directory, out sub);
            ReadOnlyCollection<AbstractPath> paths = fs.GetChildPaths(sub, regex);
            List<AbstractPath> newPaths = new List<AbstractPath>(paths.Count);
            foreach (AbstractPath path in paths)
            {
                String[] newSegments = new String[path.RawPathSegments.Length + 1];
                newSegments[0] = directory.RawPathSegments[0];
                Array.Copy(path.RawPathSegments, 0, newSegments, 1, path.RawPathSegments.Length);

                if (path is DirectoryPath)
                    newPaths.Add(new DirectoryPath(newSegments));
                else
                    newPaths.Add(new FilePath(newSegments));
            }

            return new ReadOnlyCollection<AbstractPath>(newPaths);
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }


        protected FileSystem TransformPathToMountedPath(DirectoryPath path, out DirectoryPath newPath)
        {
            if (path.RawPathSegments.Length < 1)
            {
                throw new Exception("Input path too short for transform; missed a check somewhere!");
            }
            String key = path.RawPathSegments[0];

            FileSystem fs = null;
            if (Mounts.TryGetValue(key, out fs) == false)
            {
                throw new DirectoryNotFoundException("Mount point '" + key + "' not found.");
            }

            String[] newSegments = new String[path.RawPathSegments.Length - 1];
            Array.Copy(path.RawPathSegments, 1, newSegments, 0, newSegments.Length);

            newPath = new DirectoryPath(newSegments);
            System.Diagnostics.Debug.Print(path.ToString());
            System.Diagnostics.Debug.Print(newPath.ToString());
            return fs;
        }

        protected FileSystem TransformPathToMountedPath(FilePath path, out FilePath newPath)
        {
            if (path.RawPathSegments.Length < 1)
            {
                throw new Exception("Input path too short for transform; missed a check somewhere!");
            }
            String key = path.RawPathSegments[0];

            FileSystem fs;
            if (Mounts.TryGetValue(key, out fs) == false)
            {
                throw new DirectoryNotFoundException("Mount point '" + key + "' not found.");
            }

            String[] newSegments = new String[path.RawPathSegments.Length - 1];
            Array.Copy(path.RawPathSegments, 1, newSegments, 0, newSegments.Length);

            newPath = new FilePath(newSegments);
            return fs;
        }
    }
}
