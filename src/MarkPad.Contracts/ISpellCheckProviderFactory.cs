namespace MarkPad.Contracts
{
	public interface ISpellCheckProviderFactory
	{
		ISpellCheckProvider GetProvider(ISpellingService spellingService, IDocumentView view);
	}
}
