using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ed.Filotic.FileSystems
{
    /// <summary>
    /// A thin wrapper around MountPointFileSystem to prevent users from
    /// mounting or unmounting their own mount points on the file system.
    /// </summary>
    public class WindowsPhysicalFileSystem : MountPointFileSystem
    {
        public override void Mount(FileSystem fileSystem, string mountPath)
        {
            throw new InvalidOperationException("You cannot mount/unmount on the physical file system.");
        }
        public override void Unmount(string mountPath)
        {
            throw new InvalidOperationException("You cannot mount/unmount on the physical file system.");
        }
        public override void UnmountAll()
        {
            throw new InvalidOperationException("You cannot mount/unmount on the physical file system.");
        }

        internal void _Mount(FileSystem fileSystem, String mountPath)
        {
            base.Mount(fileSystem, mountPath);
        }

        internal void _Unmount(String mountPath)
        {
            base.Unmount(mountPath);
        }

        internal void _UnmountAll()
        {
            base.UnmountAll();
        }
    }
}
