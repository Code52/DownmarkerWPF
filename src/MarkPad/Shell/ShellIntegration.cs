using System;
using System.Windows;
using System.Windows.Shell;
using Caliburn.Micro;
using MarkPad.Events;

namespace MarkPad.Shell
{
    public class ShellIntegration : IHandle<FileOpenEvent>, IDisposable
    {
        private string recent = "Recent";
        private JumpList jumpList;

        public ShellIntegration(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);

            jumpList = GetJumpList();
            jumpList.ShowRecentCategory = true;
            jumpList.ShowFrequentCategory = true;
            
            var firstFile = new JumpPath { Path = @"D:\Code\github\code52\code52website\_posts\2011-12-31-introduction.md"};
            jumpList.JumpItems.Add(firstFile);

            JumpList.AddToRecentCategory(@"D:\Code\github\code52\code52website\_posts\2012-01-02-downmarker.md");
            jumpList.Apply();
        }

        private static JumpList GetJumpList()
        {
            var list = JumpList.GetJumpList(Application.Current);
            if (list != null) return list;

            list = new JumpList();
            JumpList.SetJumpList(Application.Current, list);
            return list;
        }

        public void Handle(FileOpenEvent message)
        {
            JumpList.AddToRecentCategory(message.Path);

            jumpList.Apply();
        }

        public void Dispose()
        {
            // TODO: dispose
        }
    }
}
