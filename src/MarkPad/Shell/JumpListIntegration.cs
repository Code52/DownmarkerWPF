using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Shell;
using Caliburn.Micro;
using MarkPad.Framework.Events;
using MarkPad.Services.Interfaces;

namespace MarkPad.Shell
{
    /// <summary>
    /// Class for interacting with the Windows7 JumpList
    /// </summary>
    public class JumpListIntegration : IHandle<FileOpenEvent>, IHandle<AppStartedEvent>, IDisposable
    {
        private readonly ISettingsService settingsService;
        private JumpList jumpList;

        public JumpListIntegration(IEventAggregator eventAggregator, ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            eventAggregator.Subscribe(this);
        }

        public void Handle(FileOpenEvent message)
        {
            var item = CreateJumpListItem(message.Path);
            settingsService.AddRecentFile(message.Path);
            
            if (jumpList != null)
            {
                jumpList.JumpItems.Insert(0, item);
                jumpList.Apply();
            }
        }

        public void Handle(AppStartedEvent message)
        {
            jumpList = GetJumpList();

            PopulateJumpList(settingsService.GetRecentFiles());
        }

        public void Dispose()
        {
            settingsService.Save();
        }

        private void PopulateJumpList(IEnumerable<string> recentFiles)
        {
            foreach (var file in recentFiles.Distinct())
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
            // check for Windows7
            var os = Environment.OSVersion.Version;
            if (os.Major < 6) return null;
            if (os.Minor < 1) return null;

            var list = JumpList.GetJumpList(Application.Current);
            if (list != null) return list;

            list = new JumpList { ShowFrequentCategory = false, ShowRecentCategory = false };

            JumpList.SetJumpList(Application.Current, list);
            return list;
        }


    
    }
}
