using MarkPad.DocumentSources;
using Xunit;

namespace MarkPad.Tests.DocumentSources
{
    public class SiteContextHelperTests
    {
        [Fact]
        public void jekyll_scenario()
        {
            // arrange
            const string basePath = @"c:\Path";
            const string documentFileName = @"c:\Path\Folder\File.md";
            const string imageFileName = @"c:\Path\img\image.png";

            // act
            var result = SiteContextHelper.ToRelativePath(basePath, documentFileName, imageFileName);

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
            var result = SiteContextHelper.ToRelativePath(basePath, documentFileName, imageFileName);

            // assert
            Assert.Equal(@"\image.png", result);
        }

        [Fact]
        public void new_doc_scenario2()
        {
            // arrange
            const string basePath = @"c:\Temp\";
            const string documentFileName = @"c:\Temp\File.md";
            const string imageFileName = @"c:\Temp\image.png";

            // act
            var result = SiteContextHelper.ToRelativePath(basePath, documentFileName, imageFileName);

            // assert
            Assert.Equal(@"\image.png", result);
        }
    }
}