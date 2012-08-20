using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using MarkPad.Document.Events;
using MarkPad.Helpers;

namespace MarkPad.Document.EditorBehaviours
{
    public class PasteImageIntoDocument : IHandle<EditorPreviewKeyDownEvent>
    {
        public void Handle(EditorPreviewKeyDownEvent e)
        {
            if (e.ViewModel == null) return;
            if (Keyboard.Modifiers != ModifierKeys.Control || e.Args.Key != Key.V) return;

            var images = Clipboard.GetDataObject().GetImages();
            if (!images.Any()) return;

            var sb = new StringBuilder();

            foreach (var dataImage in images)
            {
                var relativePath = e.ViewModel.MarkpadDocument.SaveImage(dataImage.Bitmap);

                var imageMarkdown = string.Format("![{0}](/{1})",
                                Path.GetFileNameWithoutExtension(relativePath),
                                relativePath.Replace('\\', '/').TrimStart('/'));
                sb.AppendLine(imageMarkdown);
            }

            e.Editor.TextArea.Selection.ReplaceSelectionWithText(sb.ToString().Trim());
            e.Args.Handled = true;
        }
    }
}