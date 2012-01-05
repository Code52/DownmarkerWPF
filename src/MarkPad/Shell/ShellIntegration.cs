using System.Windows;
using System.Windows.Shell;
using Caliburn.Micro;
using MarkPad.Events;

namespace MarkPad.Shell
{
    public class ShellIntegration : IHandle<FileOpenEvent>
    {
        public ShellIntegration(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);

            var jumpList = JumpList.GetJumpList(Application.Current);

            jumpList.JumpItems.Add(new JumpPath { CustomCategory = "Recent Files", Path = @"C:\A.txt"  });
            jumpList.JumpItems.Add(new JumpPath { CustomCategory = "Recent Files", Path = @"C:\B.txt" });
            jumpList.JumpItems.Add(new JumpPath { CustomCategory = "Recent Files", Path = @"C:\C.txt" });
        }

        public void Handle(FileOpenEvent message)
        {
            
        }
    }
}
