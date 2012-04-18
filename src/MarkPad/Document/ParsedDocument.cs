using System;
using System.IO;
using System.Text.RegularExpressions;
using Awesomium.Core;
using MarkdownDeep;
using System.Text;

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
            string header;
            string contents;
            SplitHeaderAndContents(source, out header, out contents);

            return ToHtml(header, contents);
        }

        public static string GetBodyContents(string source)
        {
            string header;
            string contents;
            SplitHeaderAndContents(source, out header, out contents);

            return MarkdownConvert(contents);
        }

        private static string MarkdownConvert(string contents)
        {
            lock (markdown)
            {
                return markdown.Transform(contents);
            }
        }

        private static string ToHtml(string header, string contents)
        {
            var body = MarkdownConvert(contents);

            string themeName;
            var head = "";
            var scripts = "";

			scripts += GetLinkScript();

            if (TryGetHeaderValue(header, "theme", out themeName))
            {
                var path = Path.Combine(WebCore.BaseDirectory, themeName);
                foreach(var stylesheet in Directory.GetFiles(path, "*.css"))
                {
                    head += String.Format("<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}/{1}\" />\r\n", themeName, Path.GetFileName(stylesheet));
                }

                foreach (var stylesheet in Directory.GetFiles(path, "*.js"))
                {
                    scripts += String.Format("<script type=\"text/javascript\" src=\"{0}/{1}\"></script>\r\n", themeName, Path.GetFileName(stylesheet));
                }
            }

            var document = String.Format("<html>\r\n<head>\r\n{0}\r\n</head>\r\n<body>\r\n{1}\r\n{2}\r\n</body>\r\n</html>", head, body, scripts);

            return document;
        }

		static string GetLinkScript()
		{
			return @"
<script type='text/javascript'>
	window.onload = function(){
		var links = document.getElementsByTagName('a');
		for (var i = 0; i < links.length; i++) {
			var l = links[i];
			if (l.getAttribute('href')) l.target = '_blank';
		}
	};
</script>
			";
		}

        private static bool TryGetHeaderValue(string header, string key, out string value)
        {
            // TODO: Cache these?
            var match = Regex.Match(header, "^" + key + "\\s*:\\s*(.*)$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            value = match.Success ? match.Result("$1").Trim() : String.Empty;

            return match.Success;
        }

        private static void SplitHeaderAndContents(string source, out string header, out string contents)
        {
            var match = Regex.Match(source, @"^--- *\r?\n+(.*?)\r?\n+--- *\r?\n+(.*)$", 
                RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                header = match.Groups[1].Value;
                contents = match.Groups[2].Value;
                return;
            }
            header = "";
            contents = source;
        }
    }
}
