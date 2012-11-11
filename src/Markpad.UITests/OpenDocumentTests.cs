using System.IO;
using Markpad.UITests.Infrastructure;
using Xunit;

namespace Markpad.UITests
{
    public class OpenDocumentTests : MarkpadUiTest
    {
        [Fact]
        public void CanOpenDocument()
        {
            var existingDoc = Path.Combine(TemporaryTestFilesDirectory, "DocToOpen.md");
            File.WriteAllText(existingDoc, "Some content");

            var openedDocument = MainWindow.OpenDocument(existingDoc);
            
            Assert.Equal("DocToOpen", openedDocument.Title);
            Assert.Equal("Some content", openedDocument.MarkdownText);
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