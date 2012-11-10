using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.Events;
using MarkPad.Infrastructure;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.DocumentSources.FileSystem
{
    public class FileMarkdownDocumentTests
    {
        const string DocumentFilename = @"c:\Path\File.md";
        readonly FileMarkdownDocument documentUnderTest;
        readonly ISiteContext siteContext;
        readonly IDocumentFactory documentFactory;
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;
        readonly IFileSystem fileSystem;

        public FileMarkdownDocumentTests()
        {
            siteContext = Substitute.For<ISiteContext>();
            documentFactory = Substitute.For<IDocumentFactory>();
            eventAggregator = Substitute.For<IEventAggregator>();
            dialogService = Substitute.For<IDialogService>();
            fileSystem = TestObjectMother.GetFileSystem();

            documentUnderTest = CreateFileMarkdownDocument(DocumentFilename, "Content");
        }

        [Fact]
        public async Task Save_PromptsToMakeReadOnlyFileWritable()
        {
            // arrange
            var fileInfo = Substitute.For<IFileInfo>();
            fileInfo.IsReadOnly.Returns(true);
            fileSystem.FileInfo(DocumentFilename).Returns(fileInfo);

            // act
            try
            {
                await documentUnderTest.Save();
            }
            catch (TaskCanceledException)
            {}

            // assert
            string format = string.Format("{0} is readonly, what do you want to do?", DocumentFilename);
            dialogService
                .Received()
                .ShowConfirmationWithCancel("Markpad", format, null, Arg.Any<ButtonExtras>(), Arg.Any<ButtonExtras>());
        }

        [Fact]
        public async Task Save_CallsSaveAsWhenReadonlyFileIsNotMadeWritable()
        {
            // arrange
            var fileInfo = Substitute.For<IFileInfo>();
            fileInfo.IsReadOnly.Returns(true);
            fileSystem.FileInfo(DocumentFilename).Returns(fileInfo);
            dialogService
                .ShowConfirmationWithCancel(Arg.Is("Markpad"), Arg.Any<string>(), Arg.Is((string)null), Arg.Any<ButtonExtras>(), Arg.Any<ButtonExtras>())
                .Returns(false); // false is dont make writable... =\
            var savedAsDocument = TaskEx.FromResult<IMarkpadDocument>(CreateFileMarkdownDocument(@"c:\Dir\AnotherFile.md", "Content"));
            documentFactory.SaveDocumentAs(documentUnderTest).Returns(savedAsDocument);

            // act
            await documentUnderTest.Save();

            // assert
            documentFactory.Received().SaveDocumentAs(documentUnderTest).IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public async Task Save_MakeReadonlyFileWritable_ContinuesSave()
        {
            // arrange
            var fileInfo = Substitute.For<IFileInfo>();
            fileInfo.IsReadOnly.Returns(true);
            fileSystem.FileInfo(DocumentFilename).Returns(fileInfo);
            dialogService
                .ShowConfirmationWithCancel(Arg.Is("Markpad"), Arg.Any<string>(), Arg.Is((string)null), Arg.Any<ButtonExtras>(), Arg.Any<ButtonExtras>())
                .Returns(true); // true is make writable... =\

            // act
            var savedDocument = await documentUnderTest.Save();

            // assert
            Assert.False(fileInfo.IsReadOnly);
            Assert.Same(savedDocument, documentUnderTest);
            fileSystem.File.Received().WriteAllTextAsync(DocumentFilename, "Content").IgnoreAwaitForNSubstituteAssertion();
        }

        [Fact]
        public void SaveImageSavesImageInRelativeImagesFolder()
        {
            // arrange
            var bitmap = new Bitmap(1, 1);

            // act
            var file = documentUnderTest.SaveImage(bitmap);

            // assert
            string expected = Path.Combine(Path.GetDirectoryName(DocumentFilename), "File_images", "File.png");
            Assert.Equal(expected, file.FullPath);
            Assert.Equal(@"File_images\File.png", file.RelativePath);
            fileSystem.Received().SaveImagePng(bitmap, expected);
        }

        [Fact]
        public void OnlyHandlesRenameWhenOriginalFilenameMatches()
        {
            // act
            documentUnderTest.Handle(new FileRenamedEvent("DifferentFile", "SomeNewFile.md"));

            // assert
            Assert.Equal(DocumentFilename, documentUnderTest.FileName); // hasnt changed
        }

        [Fact]
        public void RenamesImagesFolderOnRename()
        {
            // arrange
            const string newFileName = @"c:\SomeNewFile.md";
            var currentImagesFolder = Path.Combine(Path.GetDirectoryName(DocumentFilename), "File_images");
            fileSystem.Directory.Exists(currentImagesFolder).Returns(true);
            var newFileInfo = Substitute.For<IFileInfo>();
            newFileInfo.Name.Returns("SomeNewFile.md");
            fileSystem.FileInfo(newFileName).Returns(newFileInfo);

            // act
            documentUnderTest.Handle(new FileRenamedEvent(DocumentFilename, newFileName));

            // assert
            const string newImagesFolder = @"c:\SomeNewFile_images";
            fileSystem.Directory.Received().Move(currentImagesFolder, newImagesFolder);
        }

        FileMarkdownDocument CreateFileMarkdownDocument(string filename, string content)
        {
            return new FileMarkdownDocument(
                filename, content, siteContext, documentFactory,
                eventAggregator, dialogService, fileSystem);
        }
    }
}
