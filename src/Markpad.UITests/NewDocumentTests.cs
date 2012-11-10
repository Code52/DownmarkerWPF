using System.IO;
using Markpad.UITests.Infrastructure;
using Xunit;

namespace Markpad.UITests
{
    public class NewDocumentTests : MarkpadUiTest
    {
        [Fact]
        public void CanCreateNewDocument()
        {
            MainWindow.NewDocument();

            Assert.Equal("New Document", MainWindow.CurrentDocument.Title);
        }

        [Fact]
        public void CanSaveNewDocument()
        {
            var newDoc = Path.Combine(TemporaryTestFilesDirectory, "CanSaveNewDoc.md");

            MainWindow
                .NewDocument()
                .SaveAs(newDoc);

            WaitWhileBusy();

            Assert.True(File.Exists(newDoc));
        }
    }
}