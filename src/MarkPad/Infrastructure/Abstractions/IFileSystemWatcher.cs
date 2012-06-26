using System;
using System.IO;

namespace MarkPad.Infrastructure.Abstractions
{
    public interface IFileSystemWatcher : IDisposable
    {
        /// <summary>
        /// A synchronous method that returns a structure that contains specific information on the change that occurred, given the type of change you want to monitor and the time (in milliseconds) to wait before timing out.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.IO.WaitForChangedResult"/> that contains specific information on the change that occurred.
        /// </returns>
        /// <param name="changeType">The <see cref="T:System.IO.WatcherChangeTypes"/> to watch for. </param><param name="timeout">The time (in milliseconds) to wait before timing out. </param><filterpriority>2</filterpriority>
        WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout);

        /// <summary>
        /// Gets or sets the type of changes to watch for.
        /// </summary>
        /// 
        /// <returns>
        /// One of the <see cref="T:System.IO.NotifyFilters"/> values. The default is the bitwise OR combination of LastWrite, FileName, and DirectoryName.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The value is not a valid bitwise OR combination of the <see cref="T:System.IO.NotifyFilters"/> values. </exception><exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The value that is being set is not valid.</exception><filterpriority>2</filterpriority>
        NotifyFilters NotifyFilter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the component is enabled.
        /// </summary>
        /// 
        /// <returns>
        /// true if the component is enabled; otherwise, false. The default is false. If you are using the component on a designer in Visual Studio 2005, the default is true.
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.FileSystemWatcher"/> object has been disposed.</exception><exception cref="T:System.PlatformNotSupportedException">The current operating system is not Microsoft Windows NT or later.</exception><exception cref="T:System.IO.FileNotFoundException">The directory specified in <see cref="P:System.IO.FileSystemWatcher.Path"/> could not be found.</exception><exception cref="T:System.ArgumentException"><see cref="P:System.IO.FileSystemWatcher.Path"/> has not been set or is invalid.</exception><filterpriority>2</filterpriority>
        bool EnableRaisingEvents { get; set; }

        /// <summary>
        /// Gets or sets the filter string used to determine what files are monitored in a directory.
        /// </summary>
        /// 
        /// <returns>
        /// The filter string. The default is "*.*" (Watches all files.)
        /// </returns>
        /// <filterpriority>2</filterpriority>
        string Filter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subdirectories within the specified path should be monitored.
        /// </summary>
        /// 
        /// <returns>
        /// true if you want to monitor subdirectories; otherwise, false. The default is false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        bool IncludeSubdirectories { get; set; }

        /// <summary>
        /// Gets or sets the size (in bytes) of the internal buffer.
        /// </summary>
        /// 
        /// <returns>
        /// The internal buffer size in bytes. The default is 8192 (8 KB).
        /// </returns>
        /// <filterpriority>2</filterpriority>
        int InternalBufferSize { get; set; }

        /// <summary>
        /// Gets or sets the path of the directory to watch.
        /// </summary>
        /// 
        /// <returns>
        /// The path to monitor. The default is an empty string ("").
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The specified path does not exist or could not be found.-or- The specified path contains wildcard characters.-or- The specified path contains invalid path characters.</exception><filterpriority>2</filterpriority>
        string Path { get; set; }

        /// <summary>
        /// Gets or sets the object used to marshal the event handler calls issued as a result of a directory change.

        /// Occurs when a file or directory in the specified <see cref="P:System.IO.FileSystemWatcher.Path"/> is changed.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        event FileSystemEventHandler Changed;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="P:System.IO.FileSystemWatcher.Path"/> is created.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        event FileSystemEventHandler Created;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="P:System.IO.FileSystemWatcher.Path"/> is deleted.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        event FileSystemEventHandler Deleted;

        /// <summary>
        /// Occurs when the internal buffer overflows.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        event ErrorEventHandler Error;

        /// <summary>
        /// Occurs when a file or directory in the specified <see cref="P:System.IO.FileSystemWatcher.Path"/> is renamed.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        event RenamedEventHandler Renamed;
    }
}