using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MarkPad.Services.Interfaces;
using NHunspell;

namespace MarkPad.Services.Implementation
{
    public class SpellingService : ISpellingService
    {
        private static Dictionary<SpellingLanguages, string> langLookup;

        static SpellingService()
        {
            langLookup = new Dictionary<SpellingLanguages, string>();
            langLookup.Add(SpellingLanguages.Australian, "en_AU");
            langLookup.Add(SpellingLanguages.Canadian, "en_CA");
            langLookup.Add(SpellingLanguages.UnitedStates, "en_US");
        }

        private Hunspell speller;

        public SpellingService()
        {
        }

        public bool Spell(string word)
        {
            if (speller == null)
                return true;

            return speller.Spell(word);
        }

        public void ClearLanguages()
        {
            speller = null;
        }

        public void SetLanguage(SpellingLanguages language)
        {
            speller = new Hunspell();

            var languageKey = langLookup[language];

            var assembly = Assembly.GetExecutingAssembly();

            var dictionaryFiles = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(
                    String.Format("{0}.Implementation.Dictionaries.{1}", assembly.GetName().Name, languageKey)));

            var affixes = dictionaryFiles.Where(name => name.EndsWith(".aff")).OrderBy(s => s);
            var dictionaries = dictionaryFiles.Where(name => name.EndsWith(".dic")).OrderBy(s => s);

            var dictionaryPairs = affixes.Zip(dictionaries, (aff, dic) => new { aff, dic });

            foreach (var pair in dictionaryPairs)
            {
                using (var affStream = assembly.GetManifestResourceStream(pair.aff))
                using (var dicStream = assembly.GetManifestResourceStream(pair.dic))
                {
                    var affBytes = new BinaryReader(affStream).ReadBytes((int)affStream.Length);
                    var dicBytes = new BinaryReader(dicStream).ReadBytes((int)dicStream.Length);

                    speller.Load(affBytes, dicBytes);
                }
            }
        }
    }
}
