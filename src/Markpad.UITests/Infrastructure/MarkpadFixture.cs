using System;
using System.IO;
using System.Reflection;
using White.Core;

namespace Markpad.UITests.Infrastructure
{
    public class MarkpadFixture : IDisposable
    {
        public MarkpadFixture()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var environmentLocation = Environment.GetEnvironmentVariable("MarkpadLocation");
            var markpadLocation = environmentLocation ?? Path.Combine(directoryName, @"..\..\..\Markpad\bin\Debug\Markpad.exe");
            Application = Application.Launch(markpadLocation);
            MainWindow = new MarkpadWindow(Application, Application.GetWindow("MarkPad"));
            TemporaryTestFilesDirectory = Path.Combine(Path.GetTempPath(), "MarkpadTest");

            if (!Directory.Exists(TemporaryTestFilesDirectory))
                Directory.CreateDirectory(TemporaryTestFilesDirectory);
        }

        public Application Application { get; private set; }
        public MarkpadWindow MainWindow { get; private set; }
        public string TemporaryTestFilesDirectory { get; private set; }

        public void Dispose()
        {
            Application.Dispose();
            Directory.Delete(TemporaryTestFilesDirectory, true);
        }
    }
}
