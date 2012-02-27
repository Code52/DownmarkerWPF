using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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

        public JumpListIntegration(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public void Handle(FileOpenEvent message)
        {
            var x = new Thread(new ParameterizedThreadStart(delegate { OpenFileAsync(message.Path); }));
            x.SetApartmentState(ApartmentState.STA);
            x.Start();
        }

        public void OpenFileAsync(string openedFile)
        {
            if (!IsWin7())
                return;

            var currentFiles = jumpList.JumpItems.OfType<JumpTask>().Select(t => t.Arguments).ToList();

            if (currentFiles.Contains(openedFile))
            {
                // find file in list
                var files = settingsService.Get<List<string>>("RecentFiles");
                var index = files.IndexOf(openedFile);
                if (index >= 0) files.RemoveAt(index);
                files.Insert(0, openedFile);
                settingsService.Set("RecentFiles", files);

                // Sometimes the settings and the jumplist can get out of sequence.
                index = currentFiles.IndexOf(openedFile);

                jumpList.JumpItems.RemoveAt(index);
                InsertFileFirst(openedFile);
            }
            else
            {
                // update settings
                var files = settingsService.Get<List<string>>("RecentFiles");
                if (files == null) files = new List<string>();

                files.Insert(0, openedFile);
                if (files.Count > 5) files.RemoveAt(5);
                settingsService.Set("RecentFiles", files);

                InsertFileFirst(openedFile);
            }
        }

        private void InsertFileFirst(string openedFile)
        {
            if (!IsWin7())
                return;

            if (jumpList != null)
            {
                var item = CreateJumpListItem(openedFile);
                jumpList.JumpItems.Insert(0, item);
                jumpList.Apply();
            }
        }

        public void Handle(AppReadyEvent message)
        {
            if (!IsWin7())
                return;

            jumpList = GetJumpList();

            var x = new Thread(new ParameterizedThreadStart(delegate { PopulateJumpList(settingsService.Get<List<string>>("RecentFiles")); }));
            x.SetApartmentState(ApartmentState.STA);
            x.Start();
        }

        public void Dispose()
        {
            settingsService.Save();
        }

        private void PopulateJumpList(IEnumerable<string> recentFiles)
        {
            if (!IsWin7())
                return;

            if (recentFiles == null) return;

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
            if (!IsWin7())
                return null;

            var path = Assembly.GetEntryAssembly().CodeBase;
            return new JumpTask
                           {
                               Arguments = file,
                               ApplicationPath = path,
                               IconResourcePath = Path.Combine(Constants.IconDir, Constants.Icons[0]),
                               Title = new FileInfo(file).Name,
                               CustomCategory = "Recent Files"
                           };
        }

        private static bool IsWin7()
        {
            // check for Windows7
            var os = Environment.OSVersion.Version;
            if (os.Major < 6) return false;
            if (os.Minor < 1) return false;

            return true;
        }

        private static JumpList GetJumpList()
        {
            if (!IsWin7())
                return null;

            var list = JumpList.GetJumpList(Application.Current);
            if (list != null) return list;

            list = new JumpList { ShowFrequentCategory = false, ShowRecentCategory = false };

            JumpList.SetJumpList(Application.Current, list);
            return list;
        }
    }
}
