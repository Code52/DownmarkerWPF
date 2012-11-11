using System.IO;
using System.Windows.Forms;
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

            Assert.True(File.Exists(newDoc));
        }

        [Fact]
        public void CanSaveNewDocumentWithPastedImage()
        {
            var newDoc = Path.Combine(TemporaryTestFilesDirectory, "CanSaveNewDocWithPastedImage.md");
            Clipboard.SetImage(Properties.Resources.icon);

            MainWindow
                .NewDocument()
                .PasteClipboard()
                .SaveAs(newDoc);

            Assert.True(File.Exists(Path.Combine(TemporaryTestFilesDirectory, @"CanSaveNewDocWithPastedImage_images\CanSaveNewDocWithPastedImage.png")));
        }
    }
}