using ApprovalTests;
using ApprovalUtilities.Utilities;
using MarkPad.Document;
using Xunit;

namespace MarkPad.Tests
{
	//[UseReporter(typeof(FileLauncherReporter))]
	public class MarkDownTest
	{
		[Fact]
		public void TestParsingBodyOnly()
		{
			var text = MarkDownWithHeader();
			var output = DocumentParser.GetBodyContents(text);
			Approvals.VerifyHtml(output);
		}
		[Fact]
		public void TestParsing()
		{
			var text = MarkDownWithHeader();
			VerifyParsing(text);
		}

		private static string MarkDownWithHeader()
		{
			var text =
				@"---
theme:TestTheme
---
# This is a header block

With a Url  [HyperLink](http://code52.org/DownmarkerWPF/) 
and some **bold** formatting and *italics*

## And a subheader

 - With a bullet point 1
 - And bullet point 2
 
 Finally there is a
 
    code { block;} =  here
    
Of course we could do `code` in the middle of the page";
			return text;
		}

		[Fact]
		public void TestParsingWithoutHeader()
		{
			VerifyParsing("This Doesn't have a header");
		}

		private static void VerifyParsing(string text)
		{
			var output = DocumentParser.Parse(text, PathUtilities.GetDirectoryForCaller());
			Approvals.VerifyHtml(output);
		}
	}
}