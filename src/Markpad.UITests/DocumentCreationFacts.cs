using Markpad.UITests.Infrastructure;
using Xunit;

namespace Markpad.UITests
{
    public class DocumentCreationFacts : MarkpadUITest
    {
        [Fact]
        public void CanCreateANewDocument()
        {
            MainWindow.NewDocument();

            Assert.Equal("New Document", MainWindow.CurrentDocument.Title);
        }
    }
}