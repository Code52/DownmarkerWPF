namespace MarkPad.Infrastructure.Abstractions
{
    public interface IFileSystemWatcherFactory
    {
        IFileSystemWatcher Create(string basePath);
    }
}