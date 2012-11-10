using System.Drawing;
using System.IO;
using System.Linq;
using MarkPad.DocumentSources;
using MarkPad.DocumentSources.NewDocument;
using MarkPad.Infrastructure;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.DocumentSources.NewDocument
{
    public class NewMarkpadDocumentTests
    {
        readonly IDocumentFactory documentFactory;
        readonly IFileSystem fileSystem;

        public NewMarkpadDocumentTests()
        {
            documentFactory = Substitute.For<IDocumentFactory>();
            fileSystem = Substitute.For<IFileSystem>();
        }

        [Fact]
        public void SaveCallsSaveAs()
        {
            // arrange
            var doc = new NewMarkpadDocument(fileSystem, documentFactory, "Title", "Content");

            // act
            doc.Save();

            // assert
            documentFactory.Received().SaveDocumentAs(doc);
        }

        [Fact]
        public void SaveImageSavesImageToTempDirectory()
        {
            // arrange
            fileSystem.GetTempPath().Returns(@"c:\Temp");
            var doc = new NewMarkpadDocument(fileSystem, documentFactory, "Title", "Content");
            fileSystem.NewFileStream(Arg.Any<string>(), FileMode.Create).Returns(new MemoryStream());
            var bitmap = new Bitmap(1, 1);

            // act
            var result = doc.SaveImage(bitmap);

            // assert
            fileSystem.Received().SaveImagePng(bitmap, @"c:\Temp\Title.png");
            Assert.Equal(@"Title.png", result.RelativePath);
            Assert.Equal(@"c:\Temp\Title.png", result.FullPath);
            Assert.False(result.Saved);
        }

        [Fact]
        public void SaveImageKeepsSavedFileReferenced()
        {
            // arrange
            fileSystem.GetTempPath().Returns(@"c:\Temp");
            var doc = new NewMarkpadDocument(fileSystem, documentFactory, "Title", "Content");
            fileSystem.NewFileStream(Arg.Any<string>(), FileMode.Create).Returns(new MemoryStream());

            // act
            doc.SaveImage(new Bitmap(1, 1));

            // assert
            Assert.Equal(1, doc.AssociatedFiles.Count());
        }
    }
}
