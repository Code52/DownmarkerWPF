using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkPad.DocumentSources
{
    public static class SiteContextHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startName">The seeding name, this can be the document name, will always return a .png filename</param>
        /// <param name="directory"></param>
        /// <returns>Absolute path of the image file</returns>
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

            return Path.Combine(directory, imageFileName);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePath"></param>
        /// <param name="fromFile"></param>
        /// <param name="toFile"></param>
        /// <returns>The relative path from the fromFile toFile (including toFile filename)</returns>
        public static string ToRelativePath(string basePath, string fromFile, string toFile)
        {
            var filename = Path.GetFileName(toFile);
            var toFilePath = Path.GetDirectoryName(toFile);
            var upFrom = fromFile.Replace(basePath, string.Empty);

            var toRelativeDirectory = toFilePath.Replace(basePath.Trim('\\', '/'), string.Empty).TrimStart('\\', '/');

            var enumerable = upFrom
                .TrimStart('\\', '/') //Get rid of starting /
                .Where(c => c == '/' || c == '\\') // select each / or \
                .Select(c => "..") // turn each into a ..
                .Concat(new[] { toRelativeDirectory, filename }); // concat with the image filename

            return string.Join("\\", enumerable); //now we join with path separator giving relative path
        }
    }
}