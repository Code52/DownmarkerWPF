using System.Deployment.Application;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace MarkPad.Updater
{
    public class UpdaterViewModel : PropertyChangedBase
    {
        public UpdaterViewModel()
        {
            UpdateState = UpdateState.Unchecked;
            CheckForUpdate();
        }

        public void CheckForUpdate()
        {
            if (Background) return;
            Background = true;

            if (UpdateState == UpdateState.UpdatePending)
            {
                ApplicationDeployment.CurrentDeployment.UpdateProgressChanged += (sender, args) =>
                {
                    Progress = args.ProgressPercentage;
                };
                ApplicationDeployment.CurrentDeployment.CheckForUpdateCompleted += (sender, args) =>
                {
                    UpdateState = UpdateState.UpToDate;
                    Background = false;
                };
                ApplicationDeployment.CurrentDeployment.UpdateAsync();
            }
            else
            {
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
    }
}