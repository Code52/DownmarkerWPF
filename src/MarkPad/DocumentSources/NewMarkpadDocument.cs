using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public class NewMarkpadDocument : MarkpadDocumentBase
    {
        readonly List<string> imagesToSave = new List<string>();

        public NewMarkpadDocument(IDocumentFactory documentFactory, string content) : 
            base("New Document", content, null, documentFactory)
        { }

        public NewMarkpadDocument(IDocumentFactory documentFactory, string title, string content) :
            base(title, content, null, documentFactory)
        { }

        public override Task<IMarkpadDocument> Save()
        {
            return SaveAs();
        }

        public override Task<IMarkpadDocument> SaveAs()
        {
            return base.SaveAs()
                .ContinueWith(t=>
                {
                    // Need to migrate temporary images to the new document
                    var markpadDocument = t.Result;
                    if (imagesToSave.Count > 0)
                    {
                        foreach (var image in imagesToSave)
                        {
                            var bitmap = (Bitmap)Image.FromFile(image);
                            markpadDocument.MarkdownContent = markpadDocument.MarkdownContent.Replace(image, markpadDocument.SaveImage(bitmap));
                        }

                        markpadDocument.Save();
                    }

                    return markpadDocument;
                });
        }

        public override Task<IMarkpadDocument> Publish()
        {
            return base.Publish()
                .ContinueWith(t=>
                {
                    // Need to migrate temporary images to the new document
                    var markpadDocument = t.Result;
                    if (imagesToSave.Count > 0)
                    {
                        foreach (var image in imagesToSave)
                        {
                            var bitmap = (Bitmap) Image.FromFile(image);
                            markpadDocument.MarkdownContent = markpadDocument.MarkdownContent
                                .Replace(image, markpadDocument.SaveImage(bitmap));
                        }

                        markpadDocument.Save();
                    }
                    return markpadDocument;
                });
        }

        public override string SaveImage(Bitmap image)
        {
            var tempPath = Path.GetTempPath();
            var imageFileName = SiteContextHelper.GetFileName(Title, tempPath);

            using (var stream = new FileStream(imageFileName, FileMode.Create))
            {
                image.Save(stream, ImageFormat.Png);
            }

            imagesToSave.Add(imageFileName);

            return SiteContextHelper.ToRelativePath(tempPath, tempPath, imageFileName);
        }

        public override string ConvertToAbsolutePaths(string htmlDocument)
        {
            return SiteContextHelper.ConvertToAbsolutePaths(htmlDocument, Path.GetTempPath());
        }
    }
}