using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NHunspell;

namespace MarkPad.Document.SpellCheck
{
    public class SpellingService : ISpellingService
    {
        static readonly Dictionary<SpellingLanguages, string> LangLookup;
        Hunspell speller;

        static SpellingService()
        {
            LangLookup = new Dictionary<SpellingLanguages, string>
            {
                {SpellingLanguages.Australian, "en_AU"},
                {SpellingLanguages.Canadian, "en_CA"},
                {SpellingLanguages.UnitedStates, "en_US"},
                {SpellingLanguages.Spain, "es_ES"}
            };
        }

        public bool Spell(string word)
        {
            return speller == null || speller.Spell(word);
        }

        public IEnumerable<string> Suggestions(string word)
        {
            return speller.Suggest(word);
        }

        public void ClearLanguages()
        {
            speller = null;
        }

        public void SetLanguage(SpellingLanguages language)
        {
            speller = new Hunspell();

            var languageKey = LangLookup[language];

            var assembly = Assembly.GetExecutingAssembly();

            var dictionaryFileStart = string.Format("{0}.Document.SpellCheck.Dictionaries.{1}", assembly.GetName().Name, languageKey);
            var dictionaryFiles = assembly
                .GetManifestResourceNames()
                .Where(name => name.StartsWith(dictionaryFileStart))
                .ToArray();

            var affixes = dictionaryFiles.Where(name => name.EndsWith(".aff")).OrderBy(s => s);
            var dictionaries = dictionaryFiles.Where(name => name.EndsWith(".dic")).OrderBy(s => s);

            var dictionaryPairs = affixes.Zip(dictionaries, (aff, dic) => new { aff, dic });

            foreach (var pair in dictionaryPairs)
            {
                using (var affStream = assembly.GetManifestResourceStream(pair.aff))
                using (var dicStream = assembly.GetManifestResourceStream(pair.dic))
                {
                    if (affStream != null && dicStream != null)
                    {
                        var affBytes = new BinaryReader(affStream).ReadBytes((int)affStream.Length);
                        var dicBytes = new BinaryReader(dicStream).ReadBytes((int)dicStream.Length);

                        speller.Load(affBytes, dicBytes);
                    }
                }
            }
        }
    }
}
