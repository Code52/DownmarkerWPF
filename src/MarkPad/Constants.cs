using System.Linq;

namespace MarkPad
{
    public static class Constants
    {
        public static readonly string[] DefaultExtensions = new[] { ".md", ".markdown", ".mdown" };

        public static string ExtensionFilter
        {
            get
            {
                var extWildcards = DefaultExtensions.Select(ext => "*" + ext).ToArray();

                return "Markdown Files (" + string.Join(", ", extWildcards) + ")|" + string.Join(";", extWildcards);
            }
        }
    }
}
