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
            const string oldFilename = @"c:\OldFile.txt";
            const string newFilename = @"c:\newFile.txt";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, oldFilename)
            {
                Name = "Test",
                Selected = true,
                IsRenaming = true
            };

            // act
            testItem.Handle(new FileRenamedEvent(oldFilename, newFilename));

            // assert
            Assert.Equal("newFile.txt", testItem.Name);
            Assert.Equal(newFilename, testItem.Path);
        }

        [Fact]
        public void undo_rename_reverts_changes()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            const string oldFilename = @"c:\OldFile.txt";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, oldFilename)
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
            Assert.Equal(oldFilename, testItem.Path);
        }

        [Fact]
        public void commit_rename_moves_file()
        {
            // arrange
            var eventAggregator = Substitute.For<IEventAggregator>();
            const string oldFilename = @"c:\OldFile.txt";
            const string newFilename = @"c:\newFile.txt";
            var testItem = new FileSystemSiteItem(eventAggregator, fileSystem, oldFilename)
            {
                Name = "Test",
                Selected = true,
                IsRenaming = true
            };

            // act
            testItem.Name = newFilename;
            testItem.CommitRename();

            // assert
            fileSystem.File.Received().Move(oldFilename, newFilename);
        }
    }
}