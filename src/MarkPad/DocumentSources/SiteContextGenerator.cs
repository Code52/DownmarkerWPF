using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Caliburn.Micro;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.Infrastructure.Abstractions;
using MarkPad.Infrastructure.DialogService;

namespace MarkPad.DocumentSources
{
    public class SiteContextGenerator : ISiteContextGenerator
    {
        readonly IFileSystemWatcherFactory fileSystemWatcherFactory;
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;
        readonly IFileSystem fileSystem;

        public SiteContextGenerator(
            IEventAggregator eventAggregator, 
            IDialogService dialogService,
            IFileSystem fileSystem, 
            IFileSystemWatcherFactory fileSystemWatcherFactory)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.fileSystem = fileSystem;
            this.fileSystemWatcherFactory = fileSystemWatcherFactory;
        }

        public ISiteContext GetContext(string filename)
        {
            var directoryName = Path.GetDirectoryName(filename);
            if (directoryName == null) return null;

            var directory = new DirectoryInfo(directoryName);
            var jekyllSiteBaseDirectory = GetJekyllSiteBaseDirectory(directory);
            if (jekyllSiteBaseDirectory != null)
            {
                return new JekyllSiteContext(eventAggregator, dialogService, fileSystem, 
                    fileSystemWatcherFactory, jekyllSiteBaseDirectory, filename);
            }

            return null;
        }

        string GetJekyllSiteBaseDirectory(DirectoryInfo startDirectory)
        {
            if (startDirectory == null)
                return null;
            if (ContainsJekyllConfigFile(startDirectory))
                return startDirectory.FullName;

            return GetJekyllSiteBaseDirectory(startDirectory.Parent);
        }

        static bool ContainsJekyllConfigFile(DirectoryInfo directory)
        {
            return directory.EnumerateFiles().Any(f => string.Equals(f.Name, "_config.yml", StringComparison.InvariantCultureIgnoreCase));
        }
    }
}