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
            var jekyllSiteBaseDirectory = GetJekyllSiteBaseDirectory(directory);
            if (jekyllSiteBaseDirectory != null)
            {
                return new JekyllSiteContext(jekyllSiteBaseDirectory, filename);
            }

            return null;
        }

        private string GetJekyllSiteBaseDirectory(DirectoryInfo startDirectory)
        {
            if (startDirectory == null)
                return null;
            if (ContainsJekyllConfigFile(startDirectory))
                return startDirectory.FullName;

            return GetJekyllSiteBaseDirectory(startDirectory.Parent);
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