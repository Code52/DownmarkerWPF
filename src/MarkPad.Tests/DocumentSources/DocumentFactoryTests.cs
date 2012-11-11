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
using MarkPad.Plugins;
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

        [Fact]
        public async Task SaveDocumentAs_CopiesAssociatedFiles()
        {
            // arrange
            dialogService.GetFileSavePath(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(@"c:\AnotherPath\Test.md");
            const string content = @"Some text

![Alt](AssociatedImage.png)";
            var fileReference = new FileReference(@"c:\Path\AssociatedImage.png", "AssociatedImage.png", true);
            var doc = new TestMarkpadDocumentBase("Title", content, null, new[]{fileReference}, documentFactory,
                                                  Substitute.For<ISiteContext>(), fileSystem);

            // act
            var document = await documentFactory.SaveDocumentAs(doc);

            // assert
            Assert.Equal(1, document.AssociatedFiles.Count());
            Assert.Equal(@"c:\AnotherPath\AssociatedImage.png", document.AssociatedFiles.Single().FullPath);
            Assert.Equal("AssociatedImage.png", document.AssociatedFiles.Single().RelativePath);
            fileSystem.File.Received().Copy(@"c:\Path\AssociatedImage.png", @"c:\AnotherPath\AssociatedImage.png");
        }

        [Fact]
        public async Task SaveDocumentAs_CopiesAssociatedFilesInRelativeDir()
        {
            // arrange
            dialogService.GetFileSavePath(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(@"c:\AnotherPath\Test.md");
            const string content = @"Some text

![Alt](RelativeDir\AssociatedImage.png)";
            var fileReference = new FileReference(@"c:\Path\RelativeDir\AssociatedImage.png", @"RelativeDir\AssociatedImage.png", true);
            var doc = new TestMarkpadDocumentBase("Title", content, null, new[] { fileReference }, documentFactory,
                                                  Substitute.For<ISiteContext>(), fileSystem);

            // act
            var document = await documentFactory.SaveDocumentAs(doc);

            // assert
            Assert.Equal(1, document.AssociatedFiles.Count());
            Assert.Equal(@"c:\AnotherPath\RelativeDir\AssociatedImage.png", document.AssociatedFiles.Single().FullPath);
            Assert.Equal(@"RelativeDir\AssociatedImage.png", document.AssociatedFiles.Single().RelativePath);
            fileSystem.File.Received().Copy(@"c:\Path\RelativeDir\AssociatedImage.png", @"c:\AnotherPath\RelativeDir\AssociatedImage.png");
        }

        [Fact]
        public async Task SaveDocumentAs_RewritesAssociatedPathConformingToImagesDirectoryConvention()
        {
            // arrange
            dialogService.GetFileSavePath(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(@"c:\AnotherPath\Test.md");
            const string content = @"Some text

![Alt](Original Title_images\AssociatedImage.png)";
            var fileReference = new FileReference(@"c:\Path\Original Title_images\AssociatedImage.png", @"Original Title_images\AssociatedImage.png", true);
            var doc = new TestMarkpadDocumentBase("Original Title", content, null, new[] { fileReference }, documentFactory,
                                                  Substitute.For<ISiteContext>(), fileSystem);

            // act
            var document = await documentFactory.SaveDocumentAs(doc);

            // assert
            Assert.Equal(1, document.AssociatedFiles.Count());
            Assert.Equal(@"c:\AnotherPath\Test_images\AssociatedImage.png", document.AssociatedFiles.Single().FullPath);
            Assert.Equal(@"Test_images\AssociatedImage.png", document.AssociatedFiles.Single().RelativePath);
            fileSystem.File.Received().Copy(@"c:\Path\Original Title_images\AssociatedImage.png", @"c:\AnotherPath\Test_images\AssociatedImage.png");
        }
    }
}
