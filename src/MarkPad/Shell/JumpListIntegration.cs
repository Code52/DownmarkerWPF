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
    public class JumpListIntegration : IHandle<FileOpenEvent>, IHandle<AppReadyEvent>, IDisposable
    {
        private readonly ISettingsService settingsService;
        private JumpList jumpList;

        public JumpListIntegration(IEventAggregator eventAggregator, ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public void Handle(FileOpenEvent message)
        {
            var openedFile = message.Path;

            var currentFiles = jumpList.JumpItems.OfType<JumpTask>().Select(t => t.Arguments);

            if (currentFiles.Contains(openedFile))
            {
                // find file in list
                var files = settingsService.GetRecentFiles();
                var index = files.IndexOf(openedFile);
                files.RemoveAt(index);
                files.Insert(0, openedFile);
                settingsService.UpdateRecentFiles(files);

                jumpList.JumpItems.RemoveAt(index);
                InsertFileFirst(openedFile);
            }
            else
            {
                // update settings
                var files = settingsService.GetRecentFiles();
                files.Insert(0, openedFile);
                if (files.Count > 5) files.RemoveAt(5);
                settingsService.UpdateRecentFiles(files);

                InsertFileFirst(openedFile);
            }
        }

        private void InsertFileFirst(string openedFile)
        {
            if (jumpList != null)
            {
                var item = CreateJumpListItem(openedFile);
                jumpList.JumpItems.Insert(0, item);
                jumpList.Apply();
            }
        }

        public void Handle(AppReadyEvent message)
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
                if (!File.Exists(file)) continue;
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
