using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Shell;
using Caliburn.Micro;
using MarkPad.Events;
using MarkPad.Services.Interfaces;

namespace MarkPad.Shell
{
    public class ShellIntegration : IHandle<FileOpenEvent>, IDisposable
    {
        private readonly ISettingsService settingsService;
        private readonly JumpList jumpList;

        public ShellIntegration(IEventAggregator eventAggregator, ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            eventAggregator.Subscribe(this);

            jumpList = GetJumpList();

            foreach (var f in settingsService.GetRecentFiles())
            {
                var path = new JumpTask
                {
                    Arguments = f,
                    IconResourcePath = Assembly.GetEntryAssembly().CodeBase,
                    ApplicationPath = Assembly.GetEntryAssembly().CodeBase,
                    Title = new FileInfo(f).Name,
                    CustomCategory = "Recent"
                };
                jumpList.JumpItems.Add(path);
            }

            jumpList.Apply();
        }

        private static JumpList GetJumpList()
        {
            var list = JumpList.GetJumpList(Application.Current);
            if (list != null) return list;

            list = new JumpList { ShowFrequentCategory = false, ShowRecentCategory = false };

            JumpList.SetJumpList(Application.Current, list);
            return list;
        }

        public void Handle(FileOpenEvent message)
        {
            settingsService.AddRecentFile(message.Path);
            JumpList.AddToRecentCategory(message.Path);

            jumpList.Apply();
        }

        public void Dispose()
        {
            // TODO: dispose
        }
    }
}
