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
    /// <summary>
    /// Class for interacting with the Windows7 JumpList
    /// </summary>
    public class JumpListIntegration : IHandle<FileOpenEvent>, IDisposable
    {
        private readonly ISettingsService settingsService;
        //private readonly JumpList jumpList;

        public JumpListIntegration(IEventAggregator eventAggregator, ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            eventAggregator.Subscribe(this);

            //jumpList = GetJumpList();

            PopulateJumpList(settingsService.GetRecentFiles());
        }

        public void Handle(FileOpenEvent message)
        {
            settingsService.AddRecentFile(message.Path);
            JumpList.AddToRecentCategory(message.Path);
            //jumpList.Apply();
        }

        public void Dispose()
        {
            settingsService.Save();
        }

        private static void PopulateJumpList(IEnumerable<string> recentFiles)
        {
            foreach (var file in recentFiles)
            {
                if (File.Exists(file))
                    JumpList.AddToRecentCategory(file);
            }

            //jumpList.Apply();
        }

        //private static JumpItem CreateJumpListItem(string file)
        //{
        //    var path = Assembly.GetEntryAssembly().CodeBase;
        //    return new JumpTask
        //                   {
        //                       Arguments = file,
        //                       IconResourcePath = path,
        //                       ApplicationPath = path,
        //                       Title = new FileInfo(file).Name,
        //                       CustomCategory = "Recent Files"
        //                   };
        //}

        //private static JumpList GetJumpList()
        //{
        //    // check for Windows7
        //    var os = Environment.OSVersion.Version;
        //    if (os.Major < 6) return null;
        //    if (os.Minor < 1) return null;

        //    var list = JumpList.GetJumpList(Application.Current);
        //    if (list != null) return list;

        //    list = new JumpList { ShowFrequentCategory = false, ShowRecentCategory = true };

        //    JumpList.SetJumpList(Application.Current, list);
        //    return list;
        //}

        
    }
}
