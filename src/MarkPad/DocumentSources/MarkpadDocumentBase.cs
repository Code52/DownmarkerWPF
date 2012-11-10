using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MarkPad.Infrastructure;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public abstract class MarkpadDocumentBase : IMarkpadDocument
    {
        readonly List<FileReference> associatedFiles = new List<FileReference>();
        readonly IDocumentFactory documentFactory;
        protected readonly IFileSystem FileSystem;

        protected MarkpadDocumentBase(
            string title, string content, 
            string saveLocation,
            IDocumentFactory documentFactory,
            ISiteContext siteContext, 
            IFileSystem fileSystem)
        {
            if (title == null) throw new ArgumentNullException("title");
            if (documentFactory == null) throw new ArgumentNullException("documentFactory");
            if (siteContext == null) throw new ArgumentNullException("siteContext");

            Title = title;
            MarkdownContent = content;
            SaveLocation = saveLocation;
            SiteContext = siteContext;
            FileSystem = fileSystem;
            this.documentFactory = documentFactory;
        }

        public string Title { get; protected set; }
        public string MarkdownContent { get; set; }

        /// <summary>
        /// Represents the location of this document, it could be a file path, a blog name, or anything that describes where this document lives
        /// </summary>
        public string SaveLocation { get; protected set; }

        public virtual ISiteContext SiteContext { get; private set; }

        protected IDocumentFactory DocumentFactory
        {
            get { return documentFactory; }
        }

        public abstract Task<IMarkpadDocument> Save();

        public virtual async Task<IMarkpadDocument> SaveAs()
        {
            return await documentFactory.SaveDocumentAs(this);
        }

        public virtual Task<IMarkpadDocument> Publish()
        {
            return documentFactory.PublishDocument(null, this);
        }

        public abstract FileReference SaveImage(Bitmap bitmap);

        public IEnumerable<FileReference> AssociatedFiles { get { return associatedFiles; } }

        public void AddFile(FileReference fileReference)
        {
            associatedFiles.Add(fileReference);
        }

        public abstract string ConvertToAbsolutePaths(string htmlDocument);

        public abstract bool IsSameItem(ISiteItem siteItem);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startName">The seeding name, this can be the document name, will always return a .png filename</param>
        /// <param name="directory"></param>
        /// <returns>Absolute path of the image file</returns>
        protected string GetFileNameBasedOnTitle(string startName, string directory)
        {
            if (!FileSystem.Directory.Exists(directory))
                FileSystem.Directory.CreateDirectory(directory);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(startName);
            if (fileNameWithoutExtension == null)
                return null;

            var fileName = fileNameWithoutExtension
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty);
            var count = 1;
            var imageFileName = fileName + ".png";

            while (FileSystem.File.Exists(Path.Combine(directory, imageFileName)))
            {
                imageFileName = string.Format("{0}{1}.png", fileName, count++);
            }

            return Path.Combine(directory, imageFileName);
        }

        protected string ConvertToAbsolutePaths(string htmlDocument, string basePath)
        {
            var matches = Regex.Matches(htmlDocument, "src=\"(?<url>(?<!http://).*?)\"", RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var replace = match.Captures[0].Value;
                var url = match.Groups["url"].Value;

                if (url.StartsWith("http://"))
                    continue;

                var filePath = Path.Combine(basePath, url.TrimStart('/', '\\'));
                if (!FileSystem.File.Exists(filePath))
                    continue;
                var base64String = Convert.ToBase64String(FileSystem.File.ReadAllBytes(filePath));
                htmlDocument = htmlDocument.Replace(replace, string.Format("src=\"data:image/png;base64,{0}\"", base64String));
            }

            return htmlDocument;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="fromFile"></param>
        /// <param name="toFile"></param>
        /// <returns>The relative path from the fromFile toFile (including toFile filename)</returns>
        protected string ToRelativePath(string basePath, string fromFile, string toFile)
        {
            var filename = Path.GetFileName(toFile);
            var toFilePath = Path.GetDirectoryName(toFile);
            var upFrom = fromFile.Replace(basePath, string.Empty);

            var toRelativeDirectory = toFilePath.Replace(basePath.Trim('\\', '/'), string.Empty).TrimStart('\\', '/');

            var enumerable = upFrom
                .TrimStart('\\', '/') //Get rid of starting /
                .Where(c => c == '/' || c == '\\') // select each / or \
                .Select(c => "..") // turn each into a ..
                .Concat(new[] { toRelativeDirectory, filename }) // concat with the image filename
                .Where(s=>!string.IsNullOrEmpty(s)); //Remove empty parts

            return string.Join("\\", enumerable); //now we join with path separator giving relative path
        }
    }
}