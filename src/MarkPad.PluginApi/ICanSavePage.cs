using System.ComponentModel.Composition;
using MarkPad.Contracts;

namespace MarkPad.Plugins
{
	[InheritedExport]
	public interface ICanSavePage : IPlugin
	{
		string SavePageLabel { get; }
		void SavePage(IDocumentViewModel documentViewModel);
	}
}
