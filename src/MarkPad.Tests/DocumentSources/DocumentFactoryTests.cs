using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.NewDocument;
using MarkPad.DocumentSources.WebSources;
using MarkPad.Infrastructure;
using MarkPad.Infrastructure.DialogService;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.DocumentSources
{
    public class DocumentFactoryTests
    {
        readonly DocumentFactory documentFactory;
        readonly IDialogService dialogService;
        readonly IEventAggregator eventAggregator;
        readonly ISiteContextGenerator siteContextGenerator;
        readonly IBlogService blogService;
        readonly IWindowManager windowManager;
        readonly Lazy<IWebDocumentService> webDocumentService;
        readonly IFileSystem fileSystem;

        public DocumentFactoryTests()
        {
            dialogService = Substitute.For<IDialogService>();
            eventAggregator = Substitute.For<IEventAggregator>();
            siteContextGenerator = Substitute.For<ISiteContextGenerator>();
            blogService = Substitute.For<IBlogService>();
            windowManager = Substitute.For<IWindowManager>();
            webDocumentService = new Lazy<IWebDocumentService>(() => Substitute.For<IWebDocumentService>());
            fileSystem = TestObjectMother.GetFileSystem();

            documentFactory = new DocumentFactory(dialogService, eventAggregator, siteContextGenerator, blogService, windowManager, webDocumentService,
                                                  fileSystem);
        }

        [Fact]
        public async Task SaveDocumentAs_SavesFile()
        {
            // arrange
            const string savePath = @"c:\Path\File.md";
            dialogService.GetFileSavePath(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(savePath);
            var doc = new NewMarkpadDocument(fileSystem, documentFactory, "Title", "Content");

            // act
            var newDocument = await documentFactory.SaveDocumentAs(doc);

            // assert
            Assert.IsType<FileMarkdownDocument>(newDocument);
            fileSystem.File.Received().WriteAllTextAsync(savePath, "Content").IgnoreAwaitForNSubstituteAssertion();
        }
        
        [Fact]
        public async Task SaveDocumentAs_SavesAssociatedFiles()
        {
            // arrange
            const string savePath = @"c:\Path\File.md";
            dialogService.GetFileSavePath(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(savePath);
            var doc = new NewMarkpadDocument(fileSystem, documentFactory, "Title", "Content");
            var reference = doc.SaveImage(new Bitmap(1, 1));

            // act
            await documentFactory.SaveDocumentAs(doc);

            // assert
            fileSystem.Received().OpenBitmap(reference.FullPath);
            // One for setup, once for saveas
            fileSystem.Received(2).SaveImagePng(Arg.Any<Bitmap>(), Arg.Any<string>());
        }

        [Fact]
        public async Task SaveDocumentAs_RewritesRelativeImagePaths()
        {
            // arrange
            const string savePath = @"c:\Path\File.md";
            dialogService.GetFileSavePath(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(savePath);
            var doc = new NewMarkpadDocument(fileSystem, documentFactory, "Title", "Content");
            var reference = doc.SaveImage(new Bitmap(1, 1));
            doc.MarkdownContent += string.Format("\r\n\r\n![Alt text]({0})", reference.RelativePath);

            // act
            var newDocument = await documentFactory.SaveDocumentAs(doc);

            // assert
            Assert.Contains("(" + newDocument.AssociatedFiles.Single().RelativePath + ")", newDocument.MarkdownContent);
        }

        [Fact]
        public async Task OpenDocument_RestoresAssociatedFiles()
        {
            // arrange
            const string toOpen = @"c:\Path\File.md";
            const string content = @"Some text

![Alt](File_images\File1.png)";
            fileSystem.File.Exists(@"c:\Path\File_images\File1.png").Returns(true);
            fileSystem.File.ReadAllTextAsync(toOpen).Returns(TaskEx.FromResult(content));
            siteContextGenerator.GetContext(toOpen).Returns(new SingleFileContext(toOpen));

            // act
            var document = await documentFactory.OpenDocument(toOpen);

            // assert
            Assert.Equal(1, document.AssociatedFiles.Count());
        }
    }
}
