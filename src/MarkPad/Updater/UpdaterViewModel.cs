using System;
using System.Deployment.Application;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace MarkPad.Updater
{
    public class UpdaterViewModel : PropertyChangedBase
    {
        IDoWorkAsyncronously asyncWork;
        IDisposable updateDownloading;

        public UpdaterViewModel()
        {
            UpdateState = UpdateState.Unchecked;
            CheckForUpdate();
        }

        public void CheckForUpdate()
        {
            if (Background) return;

            if (UpdateState == UpdateState.UpdatePending)
            {
                Background = true;
                ApplicationDeployment.CurrentDeployment.UpdateCompleted += (sender, args) => Execute.OnUIThread(() =>
                {
                    UpdateState = UpdateState.RestartNeeded;
                    updateDownloading.Dispose();
                    Background = false;
                });
                ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += (sender, args) => Execute.OnUIThread(() =>
                {
                    Progress = args.ProgressPercentage;
                    asyncWork.UpdateMessage(string.Format("Downloading update - {1:D}k of {2:D}k downloaded ({0}%)", args.ProgressPercentage, args.BytesCompleted / 1024, args.BytesTotal / 1024),
                        updateDownloading);
                });

                UpdateState = UpdateState.Downloading;
                Background = false;
                updateDownloading = asyncWork.DoingWork("Downloading update");
                ApplicationDeployment.CurrentDeployment.UpdateAsync();
            }
            else if (UpdateState == UpdateState.Unchecked)
            {
                Background = true;
                CheckForUpdatesInBackground();                
            }
        }

        void CheckForUpdatesInBackground()
        {
            Task.Factory.StartNew(() =>
            {
                if (!ApplicationDeployment.IsNetworkDeployed)
                {
                    Execute.OnUIThread(() =>
                    {
                        ErrorToolip = "Unable to check for updates, install Markpad via ClickOnce to enable updates";
                        UpdateState = UpdateState.Error;
                        Background = false;
                    });

                    return;
                }

                if (!ApplicationDeployment.CurrentDeployment.CheckForUpdate())
                {
                    Execute.OnUIThread(() =>
                    {
                        UpdateState = UpdateState.UpToDate;
                        Background = false;
                    });

                    return;
                }
                Execute.OnUIThread(() =>
                {
                    UpdateState = UpdateState.UpdatePending;
                    Background = false;
                });
            });
        }

        public int Progress { get; private set; }

        public UpdateState UpdateState { get; set; }

        public bool Background { get; set; }

        public string ErrorToolip { get; set; }

        public void Initialise(IDoWorkAsyncronously asyncWorkNotifier)
        {
            asyncWork = asyncWorkNotifier;
        }
    }
}