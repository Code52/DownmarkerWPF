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
        readonly IOpenDocumentFromWeb openDocumentFromWeb;

        public DocumentFactoryTests()
        {
            dialogService = Substitute.For<IDialogService>();
            eventAggregator = Substitute.For<IEventAggregator>();
            siteContextGenerator = Substitute.For<ISiteContextGenerator>();
            blogService = Substitute.For<IBlogService>();
            windowManager = Substitute.For<IWindowManager>();
            webDocumentService = new Lazy<IWebDocumentService>(() => Substitute.For<IWebDocumentService>());
            fileSystem = TestObjectMother.GetFileSystem();
            openDocumentFromWeb = Substitute.For<IOpenDocumentFromWeb>();

            
            documentFactory = new DocumentFactory(dialogService, eventAggregator, siteContextGenerator, blogService, windowManager, webDocumentService,
                                                  fileSystem, openDocumentFromWeb);
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
            fileSystem.Received().NewStreamWriter(savePath);
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
    }
}
