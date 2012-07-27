using System.ComponentModel.Composition;

namespace MarkPad.Plugins
{
	[InheritedExport]
	public interface ICanChangeMarkup
	{
		string ChangeMarkup(string markup);
	}
}
