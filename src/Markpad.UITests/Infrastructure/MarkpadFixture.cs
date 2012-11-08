using System.IO;
using System.Reflection;

namespace Markpad.UITests.Infrastructure
{
    public class MarkpadFixture
    {
        public string MarkpadLocation
        {
            get
            {
                var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Path.Combine(directoryName, @"..\..\..\Markpad\bin\Debug\Markpad.exe");
            }
        }

        public string MarkpadTitle
        {
            get { return "MarkPad"; }
        }
    }
}