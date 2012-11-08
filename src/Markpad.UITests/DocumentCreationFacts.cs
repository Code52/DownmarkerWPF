using Markpad.UITests.Infrastructure;
using Xunit;

namespace Markpad.UITests
{
    public class DocumentCreationFacts : MarkpadUiTest
    {
        [Fact]
        public void CanCreateANewDocument()
        {
            MainWindow.NewDocument();

            Assert.Equal("New Document", MainWindow.CurrentDocument.Title);
        }
    }
}