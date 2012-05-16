using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using MarkPad.Extensions;
using System.IO;
using ICSharpCode.AvalonEdit;

namespace MarkPad.Document.AvalonEditPreviewKeyDownHandlers
{
    public class PasteImagesUsingSiteContext : IAvalonEditPreviewKeyDownHandlers
    {
        public void Handle(DocumentViewModel viewModel, TextEditor editor, KeyEventArgs e)
        {
            if (viewModel == null) return;
            if (Keyboard.Modifiers != ModifierKeys.Control || e.Key != Key.V) return;
            if (viewModel.SiteContext == null) return;

            var images = Clipboard.GetDataObject().GetImages();
            if (!images.Any()) return;

            var sb = new StringBuilder();

            foreach (var dataImage in images)
            {
                var relativePath = viewModel.SiteContext.SaveImage(dataImage.Bitmap);

                var imageMarkdown = string.Format("![{0}](/{1})",
                                Path.GetFileNameWithoutExtension(relativePath),
                                relativePath.TrimStart('/').Replace('\\', '/'));
                sb.AppendLine(imageMarkdown);
            }

            editor.TextArea.Selection.ReplaceSelectionWithText(sb.ToString().Trim());
            e.Handled = true;
        }
    }
}