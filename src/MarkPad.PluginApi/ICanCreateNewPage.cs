using System.ComponentModel.Composition;

namespace MarkPad.Plugins
{
	[InheritedExport]
	public interface ICanCreateNewPage : IPlugin
	{
		string CreateNewPageLabel { get; }
		string CreateNewPage();
	}
}
