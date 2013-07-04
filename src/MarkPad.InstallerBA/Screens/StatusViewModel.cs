using System.Windows.Input;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace MarkPad.InstallerBA.Screens
{
    public class StatusViewModel : ViewModelBase
    {
        public StatusViewModel(ICommand installCommand)
        {
            this.InstallCommand = installCommand;
        }

        public ICommand InstallCommand { get; set; }
        public PackageState MarkPadInstallState { get; set; }

        public bool InstallMarkPad { get { return MarkPadInstallState == PackageState.Absent; } }
    }
}