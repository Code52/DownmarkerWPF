using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Windows.Threading;
using Caliburn.Micro;
using Shimmer.Client;
using wyDay.Controls;

namespace MarkPad.Updater
{
    public class UpdaterViewModel : PropertyChangedBase
    {
        private readonly IWindowManager windowManager;
        private readonly Func<UpdaterChangesViewModel> changesCreator;
        static AutomaticUpdaterBackend au;

        public int Progress { get; private set; }
        public UpdateState UpdateState { get; set; }
        public bool Background { get; set; }

        public UpdaterViewModel(IWindowManager windowManager, Func<UpdaterChangesViewModel> changesCreator)
        {
            this.windowManager = windowManager;
            this.changesCreator = changesCreator;

            // XXX: Need to find a place for this
            var updateManager = new UpdateManager(@"C:\Users\Paul\Documents\GitHub\DownmarkerWPF\src\Releases", "MarkPad", FrameworkVersion.Net40);
            var theLock = updateManager.AcquireUpdateLock();

            var update = updateManager.CheckForUpdate();

            update.Where(x => x != null).Subscribe(updateInfo =>
            {
                if (updateInfo != null)
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new System.Action(() =>
                    {
                        UpdateState = UpdateState.Downloading;
                        Background = true;
                    }));

                    var progress = new Subject<int>();
                    var applyResult = updateManager.DownloadReleases(updateInfo.ReleasesToApply, progress)
                                                   .SelectMany(_ => updateManager.ApplyReleases(updateInfo));

                    progress.Subscribe(x =>
                        Dispatcher.CurrentDispatcher.BeginInvoke(new System.Action(() => Progress = x)));

                    applyResult
                        .Finally(() =>
                        {
                            Dispatcher.CurrentDispatcher.Invoke(new System.Action(() =>
                            {
                                Background = false;
                                UpdateState = UpdateState.UpdatePending;
                            }));

                            theLock.Dispose();
                        })
                        .Subscribe(_ => { }, ex =>
                        {
                            Dispatcher.CurrentDispatcher.Invoke(new System.Action(() => UpdateState = UpdateState.Error));
                        });
                }
                else
                {
                    Dispatcher.CurrentDispatcher.BeginInvoke(new System.Action(() => UpdateState = UpdateState.UpToDate));
                    theLock.Dispose();
                }
            },
            ex =>
            {
                Dispatcher.CurrentDispatcher.Invoke(new System.Action(() => UpdateState = UpdateState.Error));
                theLock.Dispose();
            });
        }
    }
}
