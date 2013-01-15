using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using MarkPad.DocumentSources;
using MarkPad.Infrastructure;
using MarkPad.Plugins;

namespace MarkPad.Tests
{
    public class TestMarkpadDocumentBase : MarkpadDocumentBase
    {
        public TestMarkpadDocumentBase(string title, string content, string saveLocation, IEnumerable<FileReference> associatedFiles,
            IDocumentFactory documentFactory, ISiteContext siteContext, IFileSystem fileSystem) :
            base(title, content, saveLocation, associatedFiles, documentFactory, siteContext, fileSystem)
        {
        }

        public override Task<IMarkpadDocument> Save()
        {
            return TaskEx.FromResult<IMarkpadDocument>(null);
        }

        public override FileReference SaveImage(Bitmap bitmap)
        {
            return null;
        }

        public override string ConvertToAbsolutePaths(string htmlDocument)
        {
            return null;
        }

        public override bool IsSameItem(ISiteItem siteItem)
        {
            return false;
        }

        public string ToRelativePathAccessor(string basePath, string fromFile, string toFile)
        {
            return ToRelativePath(basePath, fromFile, toFile);
        }
    }
}
