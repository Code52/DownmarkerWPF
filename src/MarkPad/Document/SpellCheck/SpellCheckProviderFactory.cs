using System;
using MarkPad.Contracts;

namespace MarkPad.Document.SpellCheck
{
    public sealed class SpellCheckProviderFactory : ISpellCheckProviderFactory
    {
        private static volatile ISpellCheckProvider instance;
        private static readonly object syncRoot = new Object();

        public static ISpellCheckProvider GetProvider()
        {
            return instance;
        }

        public ISpellCheckProvider GetProvider(ISpellingService spellingService, IDocumentView view)
        {
            lock (syncRoot)
            {
                instance = new SpellCheckProvider(spellingService, view);
            }
            return instance;
        }
    }
}