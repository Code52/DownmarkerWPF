using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services.Implementation
{
    public class JekyllSiteContext : ISiteContext
    {
        private readonly string basePath;
        private readonly string filenameWithPath;

        public JekyllSiteContext(string basePath, string filename)
        {
            this.basePath = basePath;
            filenameWithPath = filename;
        }

        public string SaveImage(Bitmap image)
        {
            var absoluteImagePath = Path.Combine(basePath, "img");

            if (!Directory.Exists(absoluteImagePath))
                Directory.CreateDirectory(absoluteImagePath);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filenameWithPath);
            if (fileNameWithoutExtension == null)
                return null;

            var filename = fileNameWithoutExtension
                .Replace(" ", string.Empty)
                .Replace(".", string.Empty);
            var count = 1;
            var imageFilename = filename + ".png";

            while (File.Exists(Path.Combine(absoluteImagePath, imageFilename)))
            {
                imageFilename = string.Format("{0}{1}.png", filename, count++);
            }

            using (var stream = new FileStream(Path.Combine(absoluteImagePath, imageFilename), FileMode.Create))
            {
                image.Save(stream, ImageFormat.Png);
            }

            var enumerable = imageFilename.Replace(basePath, string.Empty).TrimStart('\\', '/') //Get rid of starting /
                .Where(c => c == '/' || c == '\\') // select each / or \
                .Select(c => "..") // turn each into a ..
                .Concat(new[] {"img", imageFilename}); // concat with the image filename
            var relativePath = string.Join("\\", enumerable); //now we join with path separator giving relative path

            return relativePath;
        }

        public string ConvertToAbsolutePaths(string htmlDocument)
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