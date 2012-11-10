namespace MarkPad.Plugins
{
    /// <summary>
    /// Represents a file associated with a document, this could be an image, or an attachment
    /// </summary>
    public class FileReference
    {
        public FileReference(string fullPath, string relativePath, bool saved   )
        {
            FullPath = fullPath;
            RelativePath = relativePath;
            Saved = saved;
        }

        /// <summary>
        /// The absolute path to the file
        /// </summary>
        public string FullPath { get; private set; }

        /// <summary>
        /// Path Relative to the document
        /// </summary>
        public string RelativePath { get; private set; }

        public bool Saved { get; set; }
    }
}