using NHunspell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkPad.Document.SpellCheck
{
    public class MultiHunspell : IDisposable
    {
        private readonly IList<Hunspell> hunspells = new List<Hunspell>();

        public void Dispose()
        {
            Clear();
        }

        public bool Spell(string word)
        {
            if (hunspells.Count == 0)
                return true;
            return hunspells.Any(x => x.Spell(word));
        }

        public IEnumerable<string> Suggestions(string word)
        {
            return hunspells.SelectMany(x => x.Suggest(word));
        }

        public void AddHunspell(Hunspell hunspell)
        {
            hunspells.Add(hunspell);
        }

        public void Clear()
        {
            foreach (var speller in hunspells)
                speller.Dispose();
            hunspells.Clear();
        }
    }
}
