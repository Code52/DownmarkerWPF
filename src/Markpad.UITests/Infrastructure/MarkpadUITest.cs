using System;
using System.IO;
using System.Windows.Automation;
using White.Core;
using White.Core.Utility;
using Xunit;

namespace Markpad.UITests.Infrastructure
{
    public class MarkpadUiTest : IUseFixture<MarkpadFixture>, IDisposable
    {
        public void SetFixture(MarkpadFixture data)
        {
            Application = Application.Launch(data.MarkpadLocation);
            MainWindow = new MarkpadWindow(Application, Application.GetWindow(data.MarkpadTitle));
            TemporaryTestFilesDirectory = Path.Combine(Path.GetTempPath(), "MarkpadTest");
            if (!Directory.Exists(TemporaryTestFilesDirectory))
                Directory.CreateDirectory(TemporaryTestFilesDirectory);
        }

        public void Dispose()
        {
            Application.Dispose();
            Directory.Delete(TemporaryTestFilesDirectory, true);
        }

        protected Application Application { get; private set; }
        protected MarkpadWindow MainWindow { get; private set; }
        protected string TemporaryTestFilesDirectory { get; private set; }

        public void WaitWhileBusy()
        {
            Retry.For(ShellIsBusy, isBusy => isBusy, (int)TimeSpan.FromSeconds(30).TotalMilliseconds);
        }

        bool ShellIsBusy()
        {
            var currentPropertyValue = MainWindow.WhiteWindow.AutomationElement.GetCurrentPropertyValue(AutomationElement.HelpTextProperty);
            return currentPropertyValue != null && ((string)currentPropertyValue).Contains("Busy");
        }
    }
}