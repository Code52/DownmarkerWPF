using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MarkdownSharp;

namespace MarkPad.Document
{
    class DocumentHeader
    {
        private readonly string _text;

        public DocumentHeader(string text)
        {
            _text = text;
        }

        public bool TryGetValue(string key, out string value)
        {
            // TODO: Cache these?
            var match = Regex.Match(_text, "^" + key + ": (.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            value = match.Success ? match.Result("$1").Trim() : String.Empty;

            return match.Success;
        }
    }

    class ParsedDocument
    {
        public DocumentHeader Header { get; private set; }
        public string Contents { get; private set; }

        public ParsedDocument(string source)
        {
            const string delimiter = "---";

            var components = source.Split(new[] { delimiter }, 2, StringSplitOptions.RemoveEmptyEntries);

            if (components.Length == 0)
            {
                Header = new DocumentHeader("");
                Contents = "";
            }
            else if (components.Length == 2)
            {
                Header = new DocumentHeader(components[0]);
                Contents = components[1];
            }
            else
            {
                Header = new DocumentHeader("");
                Contents = components[0];
            }
        }

        public string ToHtml()
        {
            var markdown = new Markdown();

            var body = markdown.Transform(Contents);

            string themeName;
            string head = "";

            if (Header.TryGetValue("theme", out themeName))
                head = String.Format(@"<link rel=""stylesheet"" type=""text/css"" href=""{0}/style.css"" />", themeName);

            var document = String.Format("<html>\r\n<head>\r\n{0}\r\n</head>\r\n<body>\r\n{1}\r\n</body>\r\n</html>", head, body);

            return document;
        }
    }
}
