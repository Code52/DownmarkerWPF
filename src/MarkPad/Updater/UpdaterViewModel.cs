using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Threading;
using Caliburn.Micro;
using Shimmer.Client;

namespace MarkPad.Updater
{
    public class UpdaterViewModel : PropertyChangedBase
    {
        private readonly IWindowManager windowManager;
        private readonly Func<UpdaterChangesViewModel> changesCreator;

        public int Progress { get; private set; }
        public UpdateState UpdateState { get; set; }
        public bool Background { get; set; }

        public UpdaterViewModel(IWindowManager windowManager, Func<UpdaterChangesViewModel> changesCreator)
        {
            this.windowManager = windowManager;
            this.changesCreator = changesCreator;

            DoUpdate();
        }

        public async void DoUpdate()
        {
            // XXX: Need to find a place for this
            var updateManager = new UpdateManager(@"C:\Users\Paul\Documents\GitHub\DownmarkerWPF\src\Releases", "MarkPad", FrameworkVersion.Net40);

            try 
            {
                var updateInfo = await updateManager.CheckForUpdateAsync(false, x => Progress += (x/3));
                if (updateInfo == null) 
                {
                    UpdateState = UpdateState.UpToDate;
                    return;
                }

                UpdateState = UpdateState.Downloading;   Background = true;
                await updateManager.DownloadReleasesAsync(updateInfo.ReleasesToApply, x => Progress += (x/3));
                await updateManager.ApplyReleasesAsync(updateInfo, x => Progress += (x/3));

                UpdateState = UpdateState.UpdatePending;
            } 
            catch (Exception) 
            {
                // NB: Probably want to log this or something
                UpdateState = UpdateState.Error;
            } 
            finally 
            {
                Background = false;
                updateManager.Dispose();
            }
        }
    }
}
