using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
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

        public string SaveImage(BitmapSource getImage)
        {
            var absoluteImagePath = Path.Combine(basePath, "img");

            var filename = Path.GetFileNameWithoutExtension(filenameWithPath);
            var count = 1;
            var imageFilename = filename + ".png";

            while (File.Exists(Path.Combine(absoluteImagePath, imageFilename)))
            {
                imageFilename = string.Format("{0}{1}.png", filename, count++);
            }

            using (var stream = new FileStream(Path.Combine(absoluteImagePath, imageFilename), FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(getImage));
                encoder.Save(stream);
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


                var base64String = Convert.ToBase64String(File.ReadAllBytes(Path.Combine(basePath, url.TrimStart('/'))));
                htmlDocument = htmlDocument.Replace(replace, string.Format("src=\"data:image/png;base64,{0}\"", base64String));
            }

            return htmlDocument;
        }
    }
}