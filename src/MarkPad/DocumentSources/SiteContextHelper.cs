using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MarkPad.DocumentSources
{
    public static class SiteContextHelper
    {
         public static string GetFileName(string startName, string directory)
         {
             if (!Directory.Exists(directory))
                 Directory.CreateDirectory(directory);

             var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(startName);
             if (fileNameWithoutExtension == null)
                 return null;

             var fileName = fileNameWithoutExtension
                 .Replace(" ", string.Empty)
                 .Replace(".", string.Empty);
             var count = 1;
             var imageFileName = fileName + ".png";

             while (File.Exists(Path.Combine(directory, imageFileName)))
             {
                 imageFileName = string.Format("{0}{1}.png", fileName, count++);
             }

             return imageFileName;
         }

        public static string ConvertToAbsolutePaths(string htmlDocument, string basePath)
        {
            var matches = Regex.Matches(htmlDocument, "src=\"(?<url>(?<!http://).*?)\"", RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                var replace = match.Captures[0].Value;
                var url = match.Groups["url"].Value;

                if (url.StartsWith("http://"))
                    continue;

                var filePath = Path.Combine(basePath, url.TrimStart('/'));
                if (!File.Exists(filePath))
                    continue;
                var base64String = Convert.ToBase64String(File.ReadAllBytes(filePath));
                htmlDocument = htmlDocument.Replace(replace, string.Format("src=\"data:image/png;base64,{0}\"", base64String));
            }

            return htmlDocument;
        }
    }
}