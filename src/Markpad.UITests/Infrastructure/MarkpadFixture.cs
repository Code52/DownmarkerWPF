using System;
using System.IO;
using System.Reflection;
using TestStack.White;

namespace MarkPad.UITests.Infrastructure
{
    public class MarkpadFixture : IDisposable
    {
        public MarkpadFixture()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var environmentLocation = Environment.GetEnvironmentVariable("MarkpadLocation");
#if DEBUG
            var markpadLocation = environmentLocation ?? Path.Combine(directoryName, @"..\..\..\Markpad\bin\Debug\Markpad.exe");
#else
            var markpadLocation = environmentLocation ?? Path.Combine(directoryName, @"..\..\..\Markpad\bin\Release\Markpad.exe");
#endif

            if (!File.Exists(markpadLocation))
            {
#if DEBUG
                markpadLocation = Path.Combine(Environment.CurrentDirectory, @"..\..\..\Markpad\bin\Debug\Markpad.exe");
#else
                markpadLocation = Path.Combine(Environment.CurrentDirectory, @"..\..\..\Markpad\bin\Release\Markpad.exe");
#endif
            }
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
