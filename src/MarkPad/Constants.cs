using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MarkPad
{
    public static class Constants
    {
        public static readonly string[] DefaultExtensions = new[] { ".md", ".markdown", ".mdown", ".mkd" };

        public static readonly string[] Icons = new[] { "markpaddoc.ico" };

        public static string ExtensionFilter
        {
            get
            {
                var extWildcards = DefaultExtensions.Select(ext => "*" + ext).ToArray();

                return "Markdown Files (" + string.Join(", ", extWildcards) + ")|" + string.Join(";", extWildcards);
            }
        }

        public static string IconDir
        {
            get
            {
                var assemblyName = Assembly.GetEntryAssembly().GetName();

                return Path.Combine(Path.GetTempPath(), String.Format("{0}.{1}", assemblyName.Name, assemblyName.Version));
            }
        }

        public const string DEFAULT_EDITOR_FONT_FAMILY = "Segoe UI";
        public const FontSizes DEFAULT_EDITOR_FONT_SIZE = FontSizes.FontSize12;
        public const int FONT_SIZE_ENUM_ADJUSTMENT = 12;
    }
}