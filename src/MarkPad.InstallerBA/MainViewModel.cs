using System.Threading.Tasks;
using System.Windows.Input;
using MarkPad.InstallerBA.Screens;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace MarkPad.InstallerBA
{
    public class MainViewModel : ViewModelBase
    {
        private readonly BootstrapperApplication bootstrapper;
        private StateEnum state;

        private readonly TaskFactory uiFactory;
        private readonly StatusViewModel statusVM;
        private readonly ProgressViewModel progressVM;

        public MainViewModel(BootstrapperApplication bootstrapper)
        {
            this.bootstrapper = bootstrapper;

            this.bootstrapper.DetectBegin += this.OnDetectBegin;
            this.bootstrapper.DetectPackageComplete += this.OnDetectPackageComplete;

            this.bootstrapper.PlanComplete += this.OnPlanComplete;
            this.bootstrapper.ApplyComplete += this.OnApplyComplete;

            state = StateEnum.None;

            uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
            statusVM = new StatusViewModel(new NextCommand(this));
            progressVM = new ProgressViewModel(bootstrapper);

            this.bootstrapper.Engine.Detect();
        }

        public object CurrentSlide { get; set; }

        private void OnDetectBegin(object sender, DetectBeginEventArgs e)
        {
            ChangeState(e.Installed ? StateEnum.Installed : StateEnum.NotInstalled);
        }

        private void OnDetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
            if (e.PackageId == "MarkPadPackageId")
                statusVM.MarkPadInstallState = e.State;
        }

        private void OnPlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (e.Status >= 0)
                bootstrapper.Engine.Apply(System.IntPtr.Zero);
        }

        private void OnApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            MoveNext();
        }

        private void InstallExecute()
        {
            bootstrapper.Engine.Plan(LaunchAction.Install);
        }

        private void UninstallExecute()
        {
            bootstrapper.Engine.Plan(LaunchAction.Uninstall);
        }

        private void ExitExecute()
        {
            MarkPadBootstrapperApplication.BootstrapperDispatcher.InvokeShutdown();
        }

        private void MoveNext()
        {
            switch (state)
            {
                case StateEnum.NotInstalled:
                    ChangeState(StateEnum.Installing);
                    InstallExecute();
                    break;

                case StateEnum.Installed:
                    ChangeState(StateEnum.Uninstalling);
                    UninstallExecute();
                    break;

                case StateEnum.Installing:
                case StateEnum.Uninstalling:
                    ChangeState(StateEnum.Exit);
                    break;

                case StateEnum.Exit:
                    ExitExecute();
                    break;
            }
        }

        public bool TryClose()
        {
            return state != StateEnum.Installing && state != StateEnum.Uninstalling;
        }

        private void ChangeState(StateEnum newState)
        {
            state = newState;

            uiFactory.StartNew(() =>
            {
                switch (state)
                {
                    case StateEnum.NotInstalled:
                    case StateEnum.Installed:
                        var status = new StatusView();
                        status.DataContext = statusVM;
                        CurrentSlide = status;
                        break;

                    case StateEnum.Installing:
                    case StateEnum.Uninstalling:
                        progressVM.Uninstalling = state == StateEnum.Uninstalling;
                        var progress = new ProgressView();
                        progress.DataContext = progressVM;
                        CurrentSlide = progress;
                        break;

                    case StateEnum.Exit:
                        var exit = new ExitView();
                        CurrentSlide = exit;
                        break;
                }
            });
        }

        public enum StateEnum
        {
            None,
            NotInstalled,
            Installing,
            Installed,
            Uninstalling,
            Exit,
        }

        private class NextCommand : ICommand
        {
            private readonly MainViewModel main;

            public NextCommand(MainViewModel main)
            {
                this.main = main;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event System.EventHandler CanExecuteChanged;

            public void Execute(object parameter)
            {
                main.MoveNext();
            }
        }
    }
}