using System;
using White.Core;
using Xunit;

namespace Markpad.UITests.Infrastructure
{
    public class MarkpadUiTest : IUseFixture<MarkpadFixture>, IDisposable
    {
        public void SetFixture(MarkpadFixture data)
        {
            Application = Application.Launch(data.MarkpadLocation);
            MainWindow = new MarkpadWindow(Application, Application.GetWindow(data.MarkpadTitle));
        }

        public void Dispose()
        {
            Application.Dispose();
        }

        protected Application Application { get; private set; }
        protected MarkpadWindow MainWindow { get; private set; }
    }
}