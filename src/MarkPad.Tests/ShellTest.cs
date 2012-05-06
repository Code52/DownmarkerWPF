using ApprovalTests.Reporters;
using ApprovalTests.Wpf;
using MarkPad.Shell;
using Xunit;

namespace MarkPad.Tests
{
	[UseReporter(typeof(DiffReporter),typeof(ClipboardReporter))]
	public class ShellTest
	{
		[Fact]
		public void TestLaunch()
		{
			
			WpfApprovals.Verify(new ShellView());
		} 
	}
}