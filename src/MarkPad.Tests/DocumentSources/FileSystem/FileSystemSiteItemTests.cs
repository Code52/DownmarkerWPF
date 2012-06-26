using System.IO.Abstractions;
using Caliburn.Micro;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.Events;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.DocumentSources.FileSystem
{
    public class FileSystemSiteItemTests
    {
        readonly IFileSystem fileSystem;

        public FileSystemSiteItemTests()
        {
            fileSystem = Substitute.For<IFileSystem>();
        }

        [Fact]
        public void renames_self_when_receives_filerenamed_event()
        {
            // arrange
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            const string oldFileName = @"c:\OldFile.txt";
            const string newFileName = @"c:\newFile.txt";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, oldFileName)
            {
                Name = "Test",
                Selected = true,
                IsRenaming = true
            };

            // act
            testItem.Handle(new FileRenamedEvent(oldFileName, newFileName));

            // assert
            Assert.Equal("newFile.txt", testItem.Name);
            Assert.Equal(newFileName, testItem.Path);
        }

        [Fact]
        public void undo_rename_reverts_changes()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            const string oldFileName = @"c:\OldFile.txt";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, oldFileName)
            {
                Name = "Test",
                Selected = true,
                IsRenaming = true
            };

            // act
            testItem.Name = "Changed";
            testItem.UndoRename();

            // assert
            Assert.Equal("OldFile.txt", testItem.Name);
            Assert.Equal(oldFileName, testItem.Path);
        }

        [Fact]
        public void commit_rename_moves_file()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            const string oldFileName = @"c:\OldFile.txt";
            const string newFileName = @"c:\newFile.txt";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, oldFileName)
            {
                Name = "Test",
                Selected = true,
                IsRenaming = true
            };

            // act
            testItem.Name = newFileName;
            testItem.CommitRename();

            // assert
            fileSystem.File.Received().Move(oldFileName, newFileName);
        }

        [Fact]
        public void inserts_new_file_into_sitecontext()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            const string oldFileName = @"c:\Site\Folder";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, oldFileName);
            testItem.Children.Add(new TestItem(eventAggregator) { Name = "Alpha.txt" });
            testItem.Children.Add(new TestItem(eventAggregator) { Name = "Gamma.txt" });

            // act
            testItem.Handle(new FileCreatedEvent(@"c:\Site\Folder\Beta.txt"));

            // assert
            Assert.Equal("Beta.txt", testItem.Children[1].Name);
        }

        [Fact]
        public void deletes_matching_child_when_deleted_event_raised()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            const string fileName = @"c:\Folder\file.txt";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, fileName)
            {
                Name = "file.txt",
                Selected = true,
                IsRenaming = true
            };
            var folder = new FileSystemSiteItem(eventAggregator, fileSystem, @"c:\Folder")
            {
                Name = "Folder",
                Children =
                {
                    testItem
                }
            };

            // act
            folder.Handle(new FileDeletedEvent(fileName));

            // assert
            Assert.Empty(folder.Children);
        }
    }
}