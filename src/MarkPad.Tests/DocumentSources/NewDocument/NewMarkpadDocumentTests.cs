using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using MarkPad.DocumentSources;
using MarkPad.DocumentSources.NewDocument;
using MarkPad.Infrastructure;
using MarkPad.Plugins;
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
        public async Task SaveAsSavesImages()
        {
            // arrange
            var bitmap = new Bitmap(1, 1);
            fileSystem.GetTempPath().Returns(@"c:\Temp");
            fileSystem.OpenBitmap(Arg.Any<string>()).Returns(bitmap);
            var doc = new NewMarkpadDocument(fileSystem, documentFactory, "Title", "Content");
            var markpadDocument = Substitute.For<IMarkpadDocument>();
            documentFactory.SaveDocumentAs(doc).Returns(TaskEx.FromResult(markpadDocument));
            doc.SaveImage(bitmap);

            // act
            await doc.Save();

            // assert
            markpadDocument.Received().SaveImage(bitmap);
        }
    }
}
