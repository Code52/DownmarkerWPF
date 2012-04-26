using System;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using MarkPad.Services.Interfaces;

namespace MarkPad.Services.Implementation
{
    public class SiteContextGenerator : ISiteContextGenerator
    {
        private readonly IEventAggregator eventAggregator;

        public SiteContextGenerator(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public ISiteContext GetContext(string filename)
        {
            var directoryName = Path.GetDirectoryName(filename);
            if (directoryName == null) return null;

            var directory = new DirectoryInfo(directoryName);
            var jekyllSiteBaseDirectory = GetJekyllSiteBaseDirectory(directory);
            if (jekyllSiteBaseDirectory != null)
            {
                return new JekyllSiteContext(eventAggregator, jekyllSiteBaseDirectory, filename);
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

        private static bool ContainsJekyllConfigFile(DirectoryInfo directory)
        {
            return directory.EnumerateFiles().Any(f => string.Equals(f.Name, "_config.yml", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}