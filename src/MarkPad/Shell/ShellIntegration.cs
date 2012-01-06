using System;
using System.Collections.Generic;
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

            PopulateJumpList(settingsService.GetRecentFiles());
        }

        public void Handle(FileOpenEvent message)
        {
            settingsService.AddRecentFile(message.Path);
            var item = CreateJumpListItem(message.Path);
            jumpList.JumpItems.Insert(0, item);

            jumpList.Apply();
        }

        public void Dispose()
        {
            settingsService.Save();
        }

        private void PopulateJumpList(IEnumerable<string> recentFiles)
        {
            foreach (var file in recentFiles)
            {
                var item = CreateJumpListItem(file);
                jumpList.JumpItems.Add(item);
            }

            jumpList.Apply();
        }

        private static JumpItem CreateJumpListItem(string file)
        {
            var path = Assembly.GetEntryAssembly().CodeBase;
            return new JumpTask
                           {
                               Arguments = file,
                               IconResourcePath = path,
                               ApplicationPath = path,
                               Title = new FileInfo(file).Name,
                               CustomCategory = "Recent Files"
                           };
        }

        private static JumpList GetJumpList()
        {
            var list = JumpList.GetJumpList(Application.Current);
            if (list != null) return list;

            list = new JumpList { ShowFrequentCategory = false, ShowRecentCategory = false };

            JumpList.SetJumpList(Application.Current, list);
            return list;
        }

        
    }
}
