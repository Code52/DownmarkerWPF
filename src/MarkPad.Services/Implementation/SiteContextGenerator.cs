using System;
using System.IO;
using System.Linq;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services.Implementation
{
    public class SiteContextGenerator : ISiteContextGenerator
    {
        public ISiteContext GetContext(string filename)
        {
            var directoryName = Path.GetDirectoryName(filename);
            if (directoryName == null) return null;

            var directory = new DirectoryInfo(directoryName);
            if (IsJekyllSite(filename, directory))
            {
                var baseDirectory = GetBaseDirectory(directory);
                return new JekyllSiteContext(baseDirectory, filename);
            }

            return null;
        }

        private string GetBaseDirectory(DirectoryInfo startDirectory)
        {
            if (ContainsJekyllConfigFile(startDirectory))
                return startDirectory.FullName;

            return GetBaseDirectory(startDirectory.Parent);
        }

        private static bool IsJekyllSite(string filename, DirectoryInfo directory)
        {
            return 
                filename.IndexOf("_posts", StringComparison.InvariantCultureIgnoreCase) != -1 ||
                ContainsJekyllConfigFile(directory);
        }

        private static bool ContainsJekyllConfigFile(DirectoryInfo directory)
        {
            return directory.EnumerateFiles().Any(f => string.Equals(f.Name, "_config.yml", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}