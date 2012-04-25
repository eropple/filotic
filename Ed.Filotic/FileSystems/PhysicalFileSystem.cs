using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ed.Filotic.Paths;

namespace Ed.Filotic.FileSystems
{
    public static class PhysicalFileSystem
    {
        public static readonly FileSystem Instance;
        static PhysicalFileSystem()
        {
            OperatingSystem os = Environment.OSVersion;
            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    Instance = new WindowsPhysicalFileSystem();
                    Refresh();
                    return;
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    Instance = new DirectoryFileSystem("/");
                    return;
                default:
                    throw new Exception("OS platform not recognized: " + os.Platform.ToString());
            }
        }

        /// <summary>
        /// Refreshes the physical file system representation. Unnecessary on
        /// Unix systems, but periodically recommended on Windows if it's possible
        /// that you're doing file operations on a mounted volume or removable
        /// disk.
        /// </summary>
        /// <remarks>
        /// Okay, this is a terrible solution but I can't find a better way to
        /// do it without resorting to WMI (and thus forcing users to load two
        /// different versions of the library depending on platform, because the
        /// DLL gets polluted with .NET-only library references).
        /// 
        /// For my own personal use cases, this isn't a big deal: I'm not exposing
        /// Filotic to end users and all my own uses are going to be in places like
        /// %MY_APP_PATH%/Content or %APPDATA%/myapp/saves/foobar - that is, in
        /// locations that don't generally change. But if somebody else has a nifty
        /// solution that doesn't require this method, I'm all for that.
        /// </remarks>
        public static void Refresh()
        {
            OperatingSystem os = Environment.OSVersion;
            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    WindowsPhysicalFileSystem fs = (WindowsPhysicalFileSystem) Instance;
                    fs._UnmountAll();

                    foreach (String drive in Environment.GetLogicalDrives())
                    {
                        String driveLetter = drive.Substring(0, 1).ToLower();

                        fs._Mount(new DirectoryFileSystem(drive), driveLetter);
                    }
                    return;
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    return;
                default:
                    throw new Exception("OS platform not recognized: " + os.Platform.ToString());
            }
        }

        /// <summary>
        /// Converts a rooted string path to a Filotic file path object rooted at the
        /// top of the PhysicalFileSystem. Will throw an exception if passed a
        /// path that is not rooted.
        /// 
        /// Note that this method will happily create paths to files that do not
        /// exist.
        /// </summary>
        /// <param name="filePath">
        /// A rooted file path. Will throw ArgumentException if not rooted.
        /// </param>
        /// <returns>
        /// A FilePath (which may or may not actually exist) that points to the
        /// given path.
        /// </returns>
        public static FilePath ToFilePath(String filePath)
        {
            if (Path.IsPathRooted(filePath) == false)
            {
                throw new ArgumentException("Path '" + filePath + "' is not a rooted path.");
            }

            OperatingSystem os = Environment.OSVersion;
            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    String[] segments = (filePath.Length == 1) ? new String[0] :
                        filePath.Substring(1).Split(Path.DirectorySeparatorChar);

                    return new FilePath(segments);
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    return new FilePath(filePath.Substring(1).Split(Path.DirectorySeparatorChar));
                default:
                    throw new Exception("OS platform not recognized: " + os.Platform.ToString());
            }
        }


        /// <summary>
        /// Converts a rooted string path to a Filotic directory path object rooted 
        /// at the top of the PhysicalFileSystem. Will throw an exception if passed a
        /// path that is not rooted.
        /// 
        /// Note that this method will happily create paths to directories that do not
        /// exist.
        /// </summary>
        /// <param name="dirPath">
        /// A rooted directory path. Will throw ArgumentException if not rooted.
        /// </param>
        /// <returns>
        /// A DirectoryPath (which may or may not actually exist) that points to the
        /// given path.
        /// </returns>
        public static DirectoryPath ToDirectoryPath(String dirPath)
        {
            if (Path.IsPathRooted(dirPath) == false)
            {
                throw new ArgumentException("Path '" + dirPath + "' is not a rooted path.");
            }

            OperatingSystem os = Environment.OSVersion;
            switch (os.Platform)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    return new DirectoryPath(WindowsPathToSegments(dirPath));
                case PlatformID.MacOSX:
                case PlatformID.Unix:
                    String[] segments = (dirPath.Length == 1) ? new String[0] :
                        dirPath.Substring(1).Split(Path.DirectorySeparatorChar);

                    return new DirectoryPath(segments);
                default:
                    throw new Exception("OS platform not recognized: " + os.Platform.ToString());
            }
        }


        /// <summary>
        /// Translates a Windows path (using foreslashes or backslashes) to a
        /// string array. The drive letter is always lower-cased.
        /// </summary>
        /// <remarks>
        /// For example, the path
        /// 
        /// D:\Users\Ed\beepdog.txt
        /// 
        /// turns into
        /// 
        /// String[] { "d", "Users", "Ed", "beepdog.txt" }
        /// </remarks>
        private static String[] WindowsPathToSegments(String realPath)
        {
            // I could have replaced in the other direction, but I expect
            // that most people will write Windows paths with backslashes.
            // Not a huge optimization, but slightly faster.
            String fixedPath = realPath.Replace("/", "\\");

            String driveLetter = fixedPath.Substring(0, 1);
            String[] rawSegments = fixedPath.Substring(3).Split('\\');

            String[] segments = new string[rawSegments.Length + 1];
            rawSegments.CopyTo(segments, 1);
            segments[0] = driveLetter;

            return segments;
        }
    }
}
