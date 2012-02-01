using System;
using System.IO;
using System.Text.RegularExpressions;
using Awesomium.Core;
using MarkdownDeep;

namespace MarkPad.Document
{
    static class DocumentParser
    {
        private static readonly Markdown markdown = new Markdown();

        static DocumentParser() 
        {
            markdown.NewWindowForExternalLinks = true;
        }

        public static string Parse(string source)
        {
            const string delimiter = "---";

            var components = source.Split(new[] { delimiter }, 2, StringSplitOptions.RemoveEmptyEntries);

            string header;
            string contents;

            if (components.Length == 0)
            {
                header = "";
                contents = "";
            }
            else if (components.Length == 2)
            {
                header = components[0];
                contents = components[1];
            }
            else
            {
                header = "";
                contents = components[0];
            }

            return ToHtml(header, contents);
        }

        public static string GetBodyContents(string source)
        {
            const string delimiter = "---";

            var components = source.Split(new[] { delimiter }, 2, StringSplitOptions.RemoveEmptyEntries);

            string contents;

            if (components.Length == 0)
            {
                contents = "";
            }
            else if (components.Length == 2)
            {
                contents = components[1];
            }
            else
            {
                contents = components[0];
            }

            return MarkdownConvert(contents);
        }

        private static string MarkdownConvert(string contents)
        {
            return markdown.Transform(contents);
        }

        private static string ToHtml(string header, string contents)
        {
            var body = MarkdownConvert(contents);

            string themeName;
            var head = "";
            var scripts = "";

            if (TryGetHeaderValue(header, "theme", out themeName))
            {
                var path = Path.Combine(WebCore.BaseDirectory, themeName);
                foreach(var stylesheet in Directory.GetFiles(path, "*.css"))
                {
                    head += String.Format("<link rel=\"stylesheet\" type=\"text/css href=\"{0}/{1}\" />\r\n", themeName, Path.GetFileName(stylesheet));
                }

                foreach (var stylesheet in Directory.GetFiles(path, "*.js"))
                {
                    scripts += String.Format("<script type=\"text/javascript\" src=\"{0}/{1}\"></script>\r\n", themeName, Path.GetFileName(stylesheet));
                }
            }

            var document = String.Format("<html>\r\n<head>\r\n{0}\r\n</head>\r\n<body>\r\n{1}\r\n{2}\r\n</body>\r\n</html>", head, body, scripts);

            return document;
        }

        private static bool TryGetHeaderValue(string header, string key, out string value)
        {
            // TODO: Cache these?
            var match = Regex.Match(header, "^" + key + "\\s*:\\s*(.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            value = match.Success ? match.Result("$1").Trim() : String.Empty;

            return match.Success;
        }
    }
}
