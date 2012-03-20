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
using MarkPad.Services;
using MarkPad.Services.Settings;

namespace MarkPad.Shell
{
    /// <summary>
    /// Class for interacting with the Windows7 JumpList
    /// </summary>
    public class JumpListIntegration : IHandle<FileOpenEvent>, IHandle<AppReadyEvent>
    {
        private readonly ISettingsProvider settingsProvider;
        private JumpList jumpList;

        public JumpListIntegration(ISettingsProvider settingsProvider)
        {
            this.settingsProvider = settingsProvider;
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
                var markpadSettings = settingsProvider.GetSettings<MarkpadSettings>();
                var index = markpadSettings.RecentFiles.IndexOf(openedFile);
                if (index >= 0) markpadSettings.RecentFiles.RemoveAt(index);
                markpadSettings.RecentFiles.Insert(0, openedFile);
                settingsProvider.SaveSettings(markpadSettings);

                // Sometimes the settings and the jumplist can get out of sequence.
                index = currentFiles.IndexOf(openedFile);

                jumpList.JumpItems.RemoveAt(index);
                InsertFileFirst(openedFile);
            }
            else
            {
                // update settings
                var markpadSettings = settingsProvider.GetSettings<MarkpadSettings>();

                markpadSettings.RecentFiles.Insert(0, openedFile);
                if (markpadSettings.RecentFiles.Count > 5) markpadSettings.RecentFiles.RemoveAt(5);
                settingsProvider.SaveSettings(markpadSettings.RecentFiles);

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

            var x = new Thread(new ParameterizedThreadStart(delegate { PopulateJumpList(settingsProvider.GetSettings<MarkpadSettings>().RecentFiles); }));
            x.SetApartmentState(ApartmentState.STA);
            x.Start();
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
