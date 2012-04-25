# Using Filotic #
Filotic is a virtual file system library for C# and .NET. It's intended to
provide a strongly typed way to perform file operations within .NET code, as
well as to provide a file system abstraction for use with virtual file systems.
The original use case of Filotic was to replace SharpFileSystem in my game mod
framework, Modbox, due to licensing issues (I don't mind the LGPL but don't
want to force compliance with it on the users of _my_ code) and due to some
design issues within the library.

## File Systems ##
The core abstraction of Filotic is **FileSystem**. FileSystem presents a
unified set of features common to all file systems and attempts to make moving
between file systems straightforward and consistent. All file systems are
patterned primarily after Unix, with a root at /. String representations of
directory paths always end in /, while string representations of file paths
never end in /.

The following file systems are considered core to Filotic:

- **PhysicalFileSystem** is an abstraction over the file system within the
computer on which the program is running. On Unix, this maps directly to the
file system, with the physical root at /. On Windows, this maps instead to a
virtual system that could be considered to be at the level of the *Computer*
window; drives in the system are mapped to /C, /D, and so on. This isn't a
user-instantiated file system, but rather one created at startup.
- **DirectoryFileSystem** is a file system that roots at a physical path. It
is used as the implementation of PhysicalFileSystem on Unix and as the
implementation of each volume on Windows.
- **MountPointFileSystem** is a purely virtual file system that allows the
consuming code to mount other file systems at any the first level within the
file system. It is used as the base implementation of PhysicalFileSystem on
Windows, with DirectoryFileSystems mounted below it.
- **MemoryFileSystem** is a file system existing only in RAM. It's intended
for "scratch" work, and provides the ability to write itself out, in its
entirety, to another file system.

In addition to the core file systems described above, there are some file
system adapters to provide different behaviors for your needs.

- **SubtreeFileSystem** is a file system that sits at the root of a defined
path on another file system. For example, when writing a game you might have
a saved game be a directory, using a SubtreeFileSystem to root it within a
DirectoryFileSystem. (This example is a little contrived because you could
just create a new DirectoryFileSystem pointing to that location, but once
you're passing around IFileSystem objects, you won't necessarily know that
it's on a physical volume.)
- **ReadOnlyFileSystem** is an adapter over an existing file system that
denies all file accesses that aren't read-only.

And, in addition to *those*, there are extended file systems that are kept
out of the main Filotic project in order to allow their inclusion only when
necessary.

- **EagerZipFileSystem** uses a ZIP file as its backing store, but loads
itself fully into memory (using a MemoryFileSystem as its base). All writes
are buffered and update into the backing ZIP file when the stream is closed;
this system allows for fast access but requires storing the uncompressed ZIP
in memory at all times.
- **LazyZipFileSystem** also uses a ZIP file as its backing store, but only
keeps the ZIP file table in memory. When a file within the file system is
accessed, the ZIP file is re-opened and only the desired file is pulled into
memory. Writes are buffered into memory and update the backing ZIP file when
the stream is closed. Reads from this file system are slower, but RAM usage
is considerably lessened.

## Path References ##
The standard System.IO libraries rely on strings as paths, which is nasty
for a number of reasons: it's harder to validate due to a lack of typing
and it's more work than it needs to be (though by no means impossible) to
deal with these paths in a cross-platform manner. Filotic uses strongly-typed
classes to represent entries within file systems.

File system paths are independent of file system. (I originally wanted to make
them bound to a file system, to allow you to invoke methods directly on the
path objects, but it turned into something of an accessibility soup and I
didn't like how it felt to work with.) Path objects are strongly typed and
descend only from a very generic interface in AbstractPath. When you've got a
FilePath object, you can open it for reading or writing; when you've got a
DirectoryPath object, you can create or destroy the directory (depending on
whether it already exists or not), you can get back a list of its children, 
etc.

You might notice that AbstractPath lacks methods like IsFile or IsDirectory.
This is intentional. You're going to need to cast to DirectoryPath or FilePath
to perform any meaningful operations anyway, so you might as well just use 'as'
and compare against null. (If this can be shown to be a performance concern in
real-world usage I'd be willing to revisit this, but I don't think it's a big
deal.)