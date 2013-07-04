using System;
using System.Linq;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace MarkPad.InstallerBA.Screens
{
    public class ProgressViewModel : ViewModelBase
    {
        public ProgressViewModel(BootstrapperApplication bootstrapper)
        {
            bootstrapper.ExecuteProgress += ExecuteProgress;
            bootstrapper.ExecuteMsiMessage += ExecuteMsiMessage;
            bootstrapper.ExecuteComplete += ExecuteComplete;

            Message = "Working";
            Progress = -1;
        }

        public string Message { get; set; }
        public int Progress { get; set; }
        public bool Uninstalling { get; set; }

        public bool IsIndeterminate { get { return Progress < 0; } }

        private void ExecuteProgress(object sender, ExecuteProgressEventArgs e)
        {
            lock (this)
            {
                this.Progress = e.OverallPercentage;
            }
        }

        private void ExecuteMsiMessage(object sender, ExecuteMsiMessageEventArgs e)
        {
            lock (this)
            {
                if (e.MessageType != InstallMessage.ActionStart)
                    return;

                var text = String.Join(":", e.Message.Split(':').Skip(3));

                var msg = String.Join(".", e.Message.Split('.').Skip(1)).Trim();

                if (Uninstalling && msg.StartsWith("Installing"))
                    msg = msg.Replace("Installing", "Uninstalling");

                if (!String.IsNullOrEmpty(msg))
                    this.Message = msg;
            }
        }

        private void ExecuteComplete(object sender, ExecuteCompleteEventArgs e)
        {
            lock (this)
            {
                this.Message = "Working";
                this.Progress = -1;
                this.Uninstalling = false;
            }
        }
    }
}