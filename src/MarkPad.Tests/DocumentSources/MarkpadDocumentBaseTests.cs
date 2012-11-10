using MarkPad.DocumentSources;
using MarkPad.Infrastructure;
using MarkPad.Plugins;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.DocumentSources
{
    public class MarkpadDocumentBaseTests
    {
        readonly TestMarkpadDocumentBase markpadDocumentBase;
        readonly IFileSystem fileSystem;
        readonly ISiteContext siteContext;
        readonly IDocumentFactory documentFactory;

        public MarkpadDocumentBaseTests()
        {
            fileSystem = Substitute.For<IFileSystem>();
            siteContext = Substitute.For<ISiteContext>();
            documentFactory = Substitute.For<IDocumentFactory>();

            markpadDocumentBase = new TestMarkpadDocumentBase("Title", "Content", null, documentFactory, siteContext, fileSystem);
        }

        [Fact]
        public void jekyll_scenario()
        {
            // arrange
            const string basePath = @"c:\Path";
            const string documentFileName = @"c:\Path\Folder\File.md";
            const string imageFileName = @"c:\Path\img\image.png";

            // act
            var result = markpadDocumentBase.ToRelativePathAccessor(basePath, documentFileName, imageFileName);

            // assert
            Assert.Equal(@"..\img\image.png", result);
        }

        [Fact]
        public void new_doc_scenario()
        {
            // arrange
            const string basePath = @"c:\Temp";
            const string documentFileName = @"c:\Temp\File.md";
            const string imageFileName = @"c:\Temp\image.png";

            // act
            var result = markpadDocumentBase.ToRelativePathAccessor(basePath, documentFileName, imageFileName);

            // assert
            Assert.Equal(@"image.png", result);
        }

        [Fact]
        public void new_doc_scenario2()
        {
            // arrange
            const string basePath = @"c:\Temp";
            const string documentFileName = @"c:\Temp\File.md";
            const string imageFileName = @"c:\Temp\image.png";

            // act
            var result = markpadDocumentBase.ToRelativePathAccessor(basePath, documentFileName, imageFileName);

            // assert
            Assert.Equal(@"image.png", result);
        }
    }
}
