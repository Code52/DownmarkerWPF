using System.ComponentModel.Composition;

namespace MarkPad.Plugins
{
	[InheritedExport]
	public interface ICanSavePage : IPlugin
	{
		string SavePageLabel { get; }
		void SavePage(IMarkpadDocument documentViewModel);
	}
}
