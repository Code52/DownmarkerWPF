using ApprovalTests;
using ApprovalTests.Reporters;
using MarkPad.Document;
using Xunit;

namespace MarkPad.Tests
{
	//[UseReporter(typeof(FileLauncherReporter))]
	public class MarkDownTest
	{
		[Fact]
		public void TestParsing()
		{
			var text = @"# This is a header block

With a Url  [HyperLink](http://code52.org/DownmarkerWPF/) 
and some **bold** formatting and *italics*

## And a subheader

 - With a bullet point 1
 - And bullet point 2
 
 Finally there is a
 
    code { block;} =  here
    
Of course we could do `code` in the middle of the page";
			var output = DocumentParser.Parse(text);
			Approvals.VerifyHtml(output);
		}
	}
}