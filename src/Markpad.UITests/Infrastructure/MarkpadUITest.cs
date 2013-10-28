using TestStack.White;
using Xunit;
using log4net.Config;

namespace MarkPad.UITests.Infrastructure
{
    public class MarkpadUiTest : IUseFixture<MarkpadFixture>
    {
        MarkpadFixture markpadFixture;

        public void SetFixture(MarkpadFixture data)
        {
            markpadFixture = data;
        }

        protected Application Application { get { return markpadFixture.Application; } }
        protected MarkpadWindow MainWindow { get { return markpadFixture.MainWindow; } }
        protected string TemporaryTestFilesDirectory { get { return markpadFixture.TemporaryTestFilesDirectory; } }
    }
}