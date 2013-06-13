using System.IO;
using System.Windows.Forms;
using MarkPad.UITests.Infrastructure;
using Xunit;

namespace MarkPad.UITests
{
    public class DocumentFacts : MarkpadUiTest
    {
        [Fact]
        public void CanCreateNewDocument()
        {
            MainWindow.NewDocument();

            Assert.Contains("New Document", MainWindow.CurrentDocument.Title);
        }

        [Fact]
        public void CanSaveNewDocument()
        {
            var newDoc = Path.Combine(TemporaryTestFilesDirectory, "CanSaveNewDoc.md");

            MainWindow.NewDocument().SaveAs(newDoc);

            Assert.True(File.Exists(newDoc));
        }

        [Fact]
        public void CanSaveNewDocumentWithPastedImage()
        {
            var newDoc = Path.Combine(TemporaryTestFilesDirectory, "CanSaveNewDocWithPastedImage.md");
            Clipboard.SetImage(Properties.Resources.icon);

            MainWindow.NewDocument().PasteClipboard().SaveAs(newDoc);

            Assert.True(File.Exists(Path.Combine(TemporaryTestFilesDirectory, @"CanSaveNewDocWithPastedImage_images\CanSaveNewDocWithPastedImage.png")));
        }

        [Fact]
        public void CanOpenDocument()
        {
            var existingDoc = Path.Combine(TemporaryTestFilesDirectory, "DocToOpen.md");
            File.WriteAllText(existingDoc, "Some content");

            var openedDocument = MainWindow.OpenDocument(existingDoc);

            Assert.Equal("DocToOpen", openedDocument.Title);
            Assert.Equal("Some content", openedDocument.Editor().MarkdownText);
        }

        [Fact]
        public void CanOpenDocumentWithAssociatedImageAndSaveAsTakingImageWithIt()
        {
            var existingDoc = Path.Combine(TemporaryTestFilesDirectory, "DocToOpenWithImage.md");
            Properties.Resources.icon.Save(Path.Combine(TemporaryTestFilesDirectory, "AssociatedImage.png"));

            File.WriteAllText(existingDoc, @"Some content with image

![Alt](AssociatedImage.png)");

            var openedDocument = MainWindow.OpenDocument(existingDoc);
            openedDocument.SaveAs(Path.Combine(TemporaryTestFilesDirectory, @"SubFolder\OpenedDocumentSavedAs.md"));

            Assert.True(File.Exists(Path.Combine(TemporaryTestFilesDirectory, @"SubFolder\AssociatedImage.png")));
        }
    }
}