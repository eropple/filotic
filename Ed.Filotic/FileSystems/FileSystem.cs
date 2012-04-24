using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Ed.Filotic.Paths;

using Stream = System.IO.Stream;

namespace Ed.Filotic.FileSystems
{
    public abstract class FileSystem : IDisposable
    {
        protected static readonly Regex AnyRegex = new Regex(".*");

        /// <summary>
        /// The root directory node of this file system. Largely decorative,
        /// but can be useful in some cases (for example, the PhysicalFileSystem
        /// on Windows).
        /// </summary>
        public abstract DirectoryPath Root { get; }
        /// <summary>
        /// A helper class containing useful methods for 
        /// </summary>
        public readonly FileSystemHelper Helper;

        protected FileSystem()
        {
            Helper = new FileSystemHelper(this);
        }

        /// <summary>
        /// Determines whether or not a path exists within the file system.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path exists; false otherwise.</returns>
        public abstract Boolean Exists(FilePath path);

        /// <summary>
        /// Determines whether or not a path exists within the file system.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True if the path exists; false otherwise.</returns>
        public abstract Boolean Exists(DirectoryPath path);

        /// <summary>
        /// Creates the given directory and all parent directories.
        /// </summary>
        /// <remarks>
        /// If the directory already exists, this is a no-op. No exception will
        /// be thrown.
        /// </remarks>
        /// <param name="directory">
        /// The directory to create.
        /// </param>
        public abstract void Create(DirectoryPath directory);
        /// <summary>
        /// Creates the listed file and returns a handle to a stream.
        /// </summary>
        /// <remarks>
        /// This method is largely superfluous and provided mostly for completeness.
        /// Its only distinguishing feature over just Open()'ing a file that does not
        /// yet exist is that it will, if createParents is true, also create parent
        /// directories. Will throw an exception if the file already exists.
        /// </remarks>
        /// <param name="file">The file to open as a stream.</param>
        /// <param name="openAsReadWrite">
        /// If true, opens using FileAccessMode.ReadWrite rather than 
        /// FileAccessMode.Write.
        /// </param>
        /// <param name="createParents">
        /// Creates any nonexistent parent directories to this path. If this is
        /// set to false, then a nonexistent parent directory will throw an
        /// exception. Failure to create any necessary parent directories will
        /// also throw an exception.
        /// </param>
        /// <returns>A Stream object corresponding to this file.</returns>
        public abstract Stream Create(FilePath file, Boolean openAsReadWrite = false, Boolean createParents = true);

        /// <summary>
        /// Deletes the given directory.
        /// </summary>
        /// <remarks>
        /// If the directory does not exist, this is a no-op. No exception will
        /// be thrown.
        /// </remarks>
        /// <param name="path">The path to delete.</param>
        /// <param name="deleteChildPaths">
        /// If true, deletes all child items in the path (the moral equivalent
        /// of "rm -rf"). If false, will throw an exception if this has any children.
        /// </param>
        public abstract void Delete(DirectoryPath path, Boolean deleteChildPaths = false);

        /// <summary>
        /// Deletes the given file.
        /// </summary>
        /// <remarks>
        /// If the file does not exist, this is a no-op. No exception will be thrown.
        /// </remarks>
        /// <param name="path">The path to delete.</param>
        public abstract void Delete(FilePath path);

        #region File Operations
        /// <summary>
        /// Opens a file according to the FileAccessMode provided and creates
        /// a stream object for manipulating it.
        /// </summary>
        /// <remarks>
        /// Open() will create a file if it does not exist and the method is
        /// passed a writable FileAccessMode. If passed FileAccessMode.Read,
        /// however, a non-existent file will trigger an exception.
        /// </remarks>
        /// <param name="file">The file to open as a stream.</param>
        /// <param name="accessMode">
        /// The access mode with which to open this file.
        /// </param>
        /// <returns>A stream corresponding to this file.</returns>
        public abstract Stream Open(FilePath file, FileAccessMode accessMode);
        #endregion

        #region Directory Searching
        /// <summary>
        /// Gets all directories that are direct children of this one.
        /// </summary>
        /// <param name="directory">The directory in which to search.</param>
        /// <returns>A collection of all child directories.</returns>
        public ReadOnlyCollection<DirectoryPath> GetChildDirectories(DirectoryPath directory)
        {
            return this.GetChildDirectories(directory, FileSystem.AnyRegex);
        }

        /// <summary>
        /// Gets all directories that are direct children of this one, filtered
        /// by the given string as a regular expression.
        /// </summary>
        /// <param name="directory">The directory in which to search.</param>
        /// <param name="regex">The regex to filter by.</param>
        /// <returns>
        /// A collection of all child directories that match the given regex.
        /// </returns>
        public ReadOnlyCollection<DirectoryPath> GetChildDirectories(DirectoryPath directory, String regex)
        {
            if (regex[0] != '^')
            {
                regex = "^" + regex;
            }

            if (regex[regex.Length - 1] != '$')
            {
                regex = regex + "$";
            }

            return this.GetChildDirectories(directory, new Regex(regex));
        }

        /// <summary>
        /// Gets all directories that are direct children of this one, filtered
        /// by the given regular expression.
        /// </summary>
        /// <remarks>
        /// The GetChildDirectories(String) method internally falls back to this
        /// one, so if you're making the same check often it may prove worthwhile
        /// to save a regex object and call this variant instead. The variant
        /// of this method with no arguments, however, uses an internally stored
        /// regex of ".*" and so using this one instead won't really be faster.
        /// </remarks>
        /// <param name="directory">The directory in which to search.</param>
        /// <param name="regex">The regex to filter by.</param>
        /// <returns>
        /// A collection of all child directories that match the given regex.
        /// </returns>
        public abstract ReadOnlyCollection<DirectoryPath> GetChildDirectories(DirectoryPath directory, Regex regex);



        /// <summary>
        /// Gets all files that are direct children of this directory.
        /// </summary>
        /// <param name="directory">The directory in which to search.</param>
        /// <returns>A collection of all child files.</returns>
        public ReadOnlyCollection<FilePath> GetChildFiles(DirectoryPath directory)
        {
            return this.GetChildFiles(directory, FileSystem.AnyRegex);
        }

        /// <summary>
        /// Gets all files that are direct children of this directory, filtered
        /// by the given string as a regular expression.
        /// </summary>
        /// <param name="directory">The directory in which to search.</param>
        /// <param name="regex">The regex to filter by.</param>
        /// <returns>
        /// A collection of all child files that match the given regex.
        /// </returns>
        public ReadOnlyCollection<FilePath> GetChildFiles(DirectoryPath directory, String regex)
        {
            if (regex[0] != '^')
            {
                regex = "^" + regex;
            }

            if (regex[regex.Length - 1] != '$')
            {
                regex = regex + "$";
            }

            return this.GetChildFiles(directory, new Regex(regex));
        }

        /// <summary>
        /// Gets all files that are direct children of this directory, filtered
        /// by the given regular expression.
        /// </summary>
        /// <remarks>
        /// The GetChildFiles(String) method internally falls back to this
        /// one, so if you're making the same check often it may prove worthwhile
        /// to save a regex object and call this variant instead. The variant
        /// of this method with no arguments, however, uses an internally stored
        /// regex of ".*" and so using this one instead won't really be faster.
        /// </remarks>
        /// <param name="directory">The directory in which to search.</param>
        /// <param name="regex">The regex to filter by.</param>
        /// <returns>
        /// A collection of all child files that match the given regex.
        /// </returns>
        public abstract ReadOnlyCollection<FilePath> GetChildFiles(DirectoryPath directory, Regex regex);


        /// <summary>
        /// Gets all direct child paths of this directory.
        /// </summary>
        /// <param name="directory">The directory in which to search.</param>
        /// <returns>A collection of all children.</returns>
        public ReadOnlyCollection<AbstractPath> GetChildPaths(DirectoryPath directory)
        {
            return this.GetChildPaths(directory, FileSystem.AnyRegex);
        }

        /// <summary>
        /// Gets all paths that are direct children of this directory, filtered
        /// by the given string as a regular expression.
        /// </summary>
        /// <param name="directory">The directory in which to search.</param>
        /// <param name="regex">The regex to filter by.</param>
        /// <returns>
        /// A collection of all child paths that match the given regex.
        /// </returns>
        public ReadOnlyCollection<AbstractPath> GetChildPaths(DirectoryPath directory, String regex)
        {
            if (regex[0] != '^')
            {
                regex = "^" + regex;
            }

            if (regex[regex.Length - 1] != '$')
            {
                regex = regex + "$";
            }

            return this.GetChildPaths(directory, new Regex(regex));
        }

        /// <summary>
        /// Gets all paths that are direct children of this directory, filtered
        /// by the given regular expression.
        /// </summary>
        /// <remarks>
        /// The GetChildPaths(String) method internally falls back to this
        /// one, so if you're making the same check often it may prove worthwhile
        /// to save a regex object and call this variant instead. The variant
        /// of this method with no arguments, however, uses an internally stored
        /// regex of ".*" and so using this one instead won't really be faster.
        /// </remarks>
        /// <param name="directory">The directory in which to search.</param>
        /// <param name="regex">The regex to filter by.</param>
        /// <returns>
        /// A collection of all child paths that match the given regex.
        /// </returns>
        public abstract ReadOnlyCollection<AbstractPath> GetChildPaths(DirectoryPath directory, Regex regex);
        #endregion



        public abstract void Dispose();
    }
}
