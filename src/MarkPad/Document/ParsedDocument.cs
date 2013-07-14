using System;
using System.IO;
using System.Text.RegularExpressions;
using MarkPad.Plugins;
using MarkPad.Settings.Models;
using MarkdownDeep;
using MarkPad.Settings;
using MarkPad.Preview;

namespace MarkPad.Document
{
    public class DocumentParser : IDocumentParser
    {
        static readonly Markdown Markdown = new Markdown();

        static DocumentParser() 
        {
            Markdown.NewWindowForExternalLinks = true;
        }

        private readonly ISettingsProvider settingsProvider;

        public DocumentParser(ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;
        }

		public string Parse(string source)
		{
		    var settings = settingsProvider.GetSettings<MarkPadSettings>();
		    Markdown.ExtraMode = settings.MarkdownExtraEnabled;

			string header;
			string contents;
			SplitHeaderAndContents(source, out header, out contents);

			const string linkScript = @"
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

			return ToHtml(header, contents, linkScript);
		}

		public string ParseClean(string source)
		{
			string header;
			string contents;
			SplitHeaderAndContents(source, out header, out contents);

			return ToHtml(header, contents, "");
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
            lock (Markdown)
            {
                return Markdown.Transform(contents);
            }
        }

        private static string ToHtml(string header, string contents, string extraScripts)
        {
			var body = MarkdownConvert(contents);

			var stylesheets = GetResources(
				header,
				"*.css",
				"<link rel=\"stylesheet\" type=\"text/css\" href=\"{0}/{1}\" />\r\n");

			var scripts = GetResources(
				header,
				"*.js",
				"<script type=\"text/javascript\" src=\"{0}/{1}\"></script>\r\n");

			var document = String.Format(
				"<html>\r\n<head>\r\n{0}\r\n</head>\r\n<body>\r\n{1}\r\n{2}{3}\r\n</body>\r\n</html>",
				stylesheets,
				body,
				scripts,
				extraScripts);

			return document;
		}

		private static string GetResources(string header, string filter, string resourceTemplate)
		{
			string themeName;
			if (!TryGetHeaderValue(header, "theme", out themeName)) return "";

			var resources = "";
			var path = Path.Combine(HtmlPreview.BaseDirectory, themeName);

			foreach (var resource in Directory.GetFiles(path, filter))
			{
				resources += String.Format(
					resourceTemplate,
					themeName,
					Path.GetFileName(resource));
			}

			return resources;
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
