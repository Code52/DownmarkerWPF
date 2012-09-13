using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Caliburn.Micro;
using MarkPad.DocumentSources.FileSystem;
using MarkPad.DocumentSources.GitHub;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.DocumentSources.WebSources;
using MarkPad.Infrastructure.Abstractions;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources
{
    public class SiteContextGenerator : ISiteContextGenerator
    {
        readonly Func<string, IMetaWeblogService> getMetaWeblog;
        readonly IFileSystemWatcherFactory fileSystemWatcherFactory;
        readonly IEventAggregator eventAggregator;
        readonly IDialogService dialogService;
        readonly IFileSystem fileSystem;
        readonly IWebDocumentService webDocumentService;
        readonly IGithubApi github;

        public SiteContextGenerator(
            IEventAggregator eventAggregator, 
            IDialogService dialogService,
            IFileSystem fileSystem, 
            IFileSystemWatcherFactory fileSystemWatcherFactory, 
            Func<string, IMetaWeblogService> getMetaWeblog, 
            IWebDocumentService webDocumentService, 
            IGithubApi github)
        {
            this.eventAggregator = eventAggregator;
            this.dialogService = dialogService;
            this.fileSystem = fileSystem;
            this.fileSystemWatcherFactory = fileSystemWatcherFactory;
            this.getMetaWeblog = getMetaWeblog;
            this.webDocumentService = webDocumentService;
            this.github = github;
        }

        public ISiteContext GetContext(string fileName)
        {
            var directoryName = Path.GetDirectoryName(fileName);
            if (directoryName == null) return null;

            var directory = new DirectoryInfo(directoryName);
            var jekyllSiteBaseDirectory = GetJekyllSiteBaseDirectory(directory);
            if (jekyllSiteBaseDirectory != null)
            {
                return new JekyllSiteContext(eventAggregator, dialogService, fileSystem, 
                    fileSystemWatcherFactory, jekyllSiteBaseDirectory);
            }

            return null;
        }

        public WebSiteContext GetWebContext(BlogSetting blog)
        {
            if (blog.WebSourceType == WebSourceType.MetaWebLog)
                return new MetaWeblogSiteContext(blog, getMetaWeblog, webDocumentService, eventAggregator);
            if (blog.WebSourceType == WebSourceType.GitHub)
                return new GithubSiteContext(blog, webDocumentService, github, eventAggregator);

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