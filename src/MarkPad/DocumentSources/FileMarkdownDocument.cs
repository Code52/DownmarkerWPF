using System;
using System.IO;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Framework;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Plugins;

namespace MarkPad.DocumentSources
{
    public class FileMarkdownDocument : MarkpadDocumentBase, IHandle<FileRenamedEvent>, IDisposable
    {
        readonly IEventAggregator eventAggregator;

        public FileMarkdownDocument(string path, string markdownContent, IDialogService dialogService, ISiteContext siteContext, IDocumentFactory documentFactory, IEventAggregator eventAggregator) : 
            base(Path.GetFileNameWithoutExtension(path), markdownContent, Path.GetDirectoryName(path), documentFactory)
        {
            this.FileName = path;
            this.eventAggregator = eventAggregator;
            SiteContext = siteContext;
            eventAggregator.Subscribe(this);
        }

        public string FileName { get; private set; }

        public override Task<IMarkpadDocument> Save()
        {
            var streamWriter = new StreamWriter(FileName);
            return streamWriter
                .WriteAsync(MarkdownContent)
                .ContinueWith<IMarkpadDocument>(t =>
                {
                    streamWriter.Dispose();

                    t.PropagateExceptions();

                    return this;
                });
        }

        public void Handle(FileRenamedEvent message)
        {
            if (FileName == message.OriginalFileName)
            {
                FileName = message.NewFileName;
                Title = new FileInfo(FileName).Name;
            }
        }

        public void Dispose()
        {
            eventAggregator.Unsubscribe(this);
        }
    }
}