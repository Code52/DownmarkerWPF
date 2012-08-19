namespace MarkPad.Infrastructure.Abstractions
{
    public class FileSystemWatcherFactory : IFileSystemWatcherFactory
    {
        public IFileSystemWatcher Create(string basePath)
        {
            return new FileSystemWatcherWrapper(basePath);
        }
    }
}