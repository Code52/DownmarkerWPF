using System.ComponentModel.Composition;

namespace MarkPad.Contracts
{
    [InheritedExport]
	public interface ISpellCheckProviderFactory
	{
		ISpellCheckProvider GetProvider(ISpellingService spellingService, IDocumentView view);
	}
}
