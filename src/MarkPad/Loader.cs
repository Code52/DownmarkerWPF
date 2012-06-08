using System;
using System.IO;
using System.Reflection;

namespace MarkPad
{
    internal class Loader
    {
        [STAThread]
        public static void Main()
        {
            var directoryName = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            if (directoryName != null && Directory.GetCurrentDirectory() != directoryName)
                Directory.SetCurrentDirectory(directoryName);

            App.Start();
        }
    }
}
