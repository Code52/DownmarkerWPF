using MarkPad.Contracts;

namespace MarkPad.Document.SpellCheck
{
    public class SpellCheckProviderFactory : ISpellCheckProviderFactory
    {
        public ISpellCheckProvider GetProvider(ISpellingService spellingService, IDocumentView view)
        {
            return new SpellCheckProvider(spellingService, view);
        }
    }
}