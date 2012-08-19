using System.IO;
using System.IO.Abstractions;
using Caliburn.Micro;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.Events;
using MarkPad.Infrastructure.Abstractions;
using MarkPad.Infrastructure.DialogService;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.DocumentSources.FileSystem
{
    public class JekyllSiteContextTests
    {
        readonly string filename;
        readonly string basePath;
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;
        readonly IFileSystem fileSystem;
        readonly IFileSystemWatcherFactory fileSystemWatcherFactory;
        readonly IFileSystemWatcher fileSystemWatcher;
        readonly JekyllSiteContext jekyllContext;

        public JekyllSiteContextTests()
        {
            filename = @"C:\Site\Index.md";
            basePath = @"C:\Site\";
            eventAggregator = Substitute.For<IEventAggregator>();
            dialogService = Substitute.For<IDialogService>();
            fileSystem = Substitute.For<IFileSystem>();
            fileSystemWatcherFactory = Substitute.For<IFileSystemWatcherFactory>();
            fileSystemWatcher = Substitute.For<IFileSystemWatcher>();
            fileSystemWatcherFactory.Create(Arg.Any<string>()).Returns(fileSystemWatcher);

            jekyllContext = new JekyllSiteContext(
                eventAggregator,
                dialogService,
                fileSystem,
                fileSystemWatcherFactory,
                basePath);
        }

        [Fact]
        public void creates_filesystem_watcher_for_base_directory()
        {
            fileSystemWatcherFactory.Received().Create(basePath);
            Assert.True(fileSystemWatcher.IncludeSubdirectories);
        }

        [Fact]
        public void raises_file_renamed_event_when_watcher_notifies_of_rename()
        {
            // arrange
            const string newFileName = @"C:\Site\Index2.md";

            // act
            var renamedEventArgs = new RenamedEventArgs(WatcherChangeTypes.Renamed, basePath, "Index2.md", "Index.md");  
            fileSystemWatcher.Renamed += Raise.Event<RenamedEventHandler>(null, renamedEventArgs);

            // assert
            eventAggregator.Received().Publish(Arg.Is<FileRenamedEvent>(f=>f.NewFileName == newFileName && f.OriginalFileName == filename));
        }

        [Fact]
        public void dispose_cleans_up_filesystem_watcher()
        {
            // act
            jekyllContext.Dispose();

            // assert
            fileSystemWatcher.Received().Dispose();
        }

        [Fact]
        public void dispose_disposes_all_site_items()
        {
            // arrange
            var testItem = new TestItem(eventAggregator);
            jekyllContext.Items.Add(testItem);

            // act
            jekyllContext.Dispose();

            // assert
            Assert.True(testItem.Disposed);
        }

        [Fact]
        public void new_file_raises_file_created_event_for_folder()
        {
            // arrange
            var createdEvent = new FileSystemEventArgs(WatcherChangeTypes.Created, basePath, "NewFolder");

            // act
            fileSystemWatcher.Created += Raise.Event<FileSystemEventHandler>(null, createdEvent);

            // assert
            eventAggregator.Received().Publish(Arg.Is<FileCreatedEvent>(c=>c.FullPath == @"C:\Site\NewFolder"));
        }

        [Fact]
        public void deleted_file_raises_deleted_event_for_folder()
        {
            // arrange
            var deletedEvent = new FileSystemEventArgs(WatcherChangeTypes.Deleted, basePath, "File.md");

            // act
            fileSystemWatcher.Deleted += Raise.Event<FileSystemEventHandler>(null, deletedEvent);

            // assert
            eventAggregator.Received().Publish(Arg.Is<FileDeletedEvent>(c => c.FileName == @"C:\Site\File.md"));
        }

        [Fact]
        public void deletes_matching_child_when_deleted_event_raised()
        {
            // arrange
            const string fileName = @"c:\Folder\file.txt";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, fileName)
            {
                Name = "file.txt",
                Selected = true,
                IsRenaming = true
            };
            jekyllContext.Items.Add(testItem);

            // act
            jekyllContext.Handle(new FileDeletedEvent(fileName));

            // assert
            Assert.Empty(jekyllContext.Items);
        }
    }
}