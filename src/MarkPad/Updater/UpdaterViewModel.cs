using System;
using System.ComponentModel;
using Caliburn.Micro;
using MarkPad.Services.Interfaces;
using wyDay.Controls;

namespace MarkPad.Updater
{
    public class UpdaterViewModel : INotifyPropertyChanged
    {
        private readonly IWindowManager windowManager;
        private readonly Func<UpdaterChangesViewModel> changesCreator;
        private readonly IDialogService dialogService;
        public event PropertyChangedEventHandler PropertyChanged;
        static AutomaticUpdaterBackend au;
        public int Progress { get; private set; }
        public UpdateState UpdateState { get; set; }
        public UpdaterViewModel(IWindowManager windowManager, Func<UpdaterChangesViewModel> changesCreator )
        {
            this.windowManager = windowManager;
            this.changesCreator = changesCreator;

            au = new AutomaticUpdaterBackend
            {
                GUID = "code52-markpad",
                UpdateType = UpdateType.CheckAndDownload,
            };

            au.ProgressChanged += AuProgressChanged;
            au.ReadyToBeInstalled += AuReadyToBeInstalled;
            au.UpToDate += AuUpToDate;
            au.UpdateAvailable += AuUpdateAvailable;
            au.UpdateSuccessful += AuUpdateSuccessful;
            
            au.Initialize();
            au.AppLoaded();
            SetUpdateFlag();
        }

        void AuUpdateAvailable(object sender, EventArgs e)
        {
            SetUpdateFlag();
        }

        void AuUpToDate(object sender, SuccessArgs e)
        {
            SetUpdateFlag();
        }

        void AuProgressChanged(object sender, int progress)
        {
            Progress = progress;
        }

        private void AuUpdateSuccessful(object sender, SuccessArgs e)
        {
            SetUpdateFlag();
        }

        private void AuReadyToBeInstalled(object sender, EventArgs e)
        {
            SetUpdateFlag();
        }

        public void CheckForUpdate()
        {
            switch (au.UpdateStepOn)
            {
                case UpdateStepOn.UpdateReadyToInstall:
                    var vm = changesCreator();
                    vm.Message = au.Changes;
                    windowManager.ShowDialog(vm);
                    if (!vm.WasCancelled)
                        au.InstallNow();
                    return;
                    break;
                case UpdateStepOn.Nothing:
                    au.ForceCheckForUpdate(true);
                    break;

            }
        }

        private void SetUpdateFlag()
        {
            switch (au.UpdateStepOn)
            {
                case UpdateStepOn.ExtractingUpdate:
                case UpdateStepOn.DownloadingUpdate:
                    UpdateState = UpdateState.Downloading;
                    break;

                case UpdateStepOn.UpdateDownloaded:
                case UpdateStepOn.UpdateAvailable:                
                    au.InstallNow();
                    break;

                case UpdateStepOn.UpdateReadyToInstall:
                    UpdateState = UpdateState.UpdatePending;
                    break;

                default:
                    UpdateState = UpdateState.Unchecked;
                    break;
            }
        }
    }
}
