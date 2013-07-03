using System.ComponentModel;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace MarkPad.InstallerBA
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly BootstrapperApplication bootstrapper;
        private StateEnum state;

        public MainViewModel(BootstrapperApplication bootstrapper)
        {
            this.bootstrapper = bootstrapper;

            this.bootstrapper.DetectBegin += this.OnDetectBegin;
            this.bootstrapper.DetectPackageComplete += this.OnDetectPackageComplete;

            this.bootstrapper.PlanComplete += this.OnPlanComplete;
            this.bootstrapper.ApplyComplete += this.OnApplyComplete;

            state = StateEnum.None;

            this.bootstrapper.Engine.Detect();
        }

        private void OnDetectBegin(object sender, DetectBeginEventArgs e)
        {
            state = e.Installed ? StateEnum.Installed : StateEnum.NotInstalled;
        }

        private void OnDetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
            //if (e.PackageId == "evORElutionPackageId")
            //    statusVM.EvorelutionState = e.State;
        }

        private void OnPlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (e.Status >= 0)
                bootstrapper.Engine.Apply(System.IntPtr.Zero);
        }

        private void OnApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            MoveNext(null);
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

        public void MoveNext(object o)
        {
            switch (state)
            {
                case StateEnum.NotInstalled:
                    this.state = StateEnum.Installing;
                    InstallExecute();
                    break;

                case StateEnum.Installed:
                    this.state = StateEnum.Uninstalling;
                    UninstallExecute();
                    break;

                case StateEnum.Installing:
                case StateEnum.Uninstalling:
                    this.state = StateEnum.Exit;
                    break;

                case StateEnum.Exit:
                    ExitExecute();
                    break;
            }
        }

#pragma warning disable 67

        public event PropertyChangedEventHandler PropertyChanged;

#pragma warning restore 67

        public enum StateEnum
        {
            None,
            NotInstalled,
            Installing,
            Installed,
            Uninstalling,
            Exit,
        }
    }
}