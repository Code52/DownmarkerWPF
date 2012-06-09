using System.ComponentModel.Composition;
using MarkPad.Contracts;

namespace MarkPad.Document.SpellCheck
{
    [Export(typeof(ISpellCheckProviderFactory))]
    public class SpellCheckProviderFactory : ISpellCheckProviderFactory
    {
        public ISpellCheckProvider GetProvider(ISpellingService spellingService, IDocumentView view)
        {
            return new SpellCheckProvider(spellingService, view);
        }
    }
}