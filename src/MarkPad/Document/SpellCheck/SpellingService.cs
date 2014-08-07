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
        readonly MultiHunspell speller = new MultiHunspell();
        SpellingLanguages currentLanguage;

        static SpellingService()
        {
            LangLookup = new Dictionary<SpellingLanguages, string>
            {
                {SpellingLanguages.Australian, "en_AU"},
                {SpellingLanguages.Canadian, "en_CA"},
                {SpellingLanguages.UnitedStates, "en_US"},
                {SpellingLanguages.UnitedKingdom, "en_GB"},
                {SpellingLanguages.Spain, "es_ES"},
                {SpellingLanguages.Germany, "de_DE"}
            };
        }

        public bool Spell(string word)
        {
            return speller.Spell(word);
        }

        public IEnumerable<string> Suggestions(string word)
        {
            return speller.Suggestions(word);
        }

        public void ClearLanguages()
        {
            speller.Clear();
        }

        public void SetLanguage(SpellingLanguages language)
        {
            currentLanguage = language;
            speller.Clear();

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

                        var newSpeller = new Hunspell(affBytes, dicBytes);
                        speller.AddHunspell(newSpeller);
                    }
                }
            }

            var customDictionaryPath = GetCustomDictionaryPath();
            if (File.Exists(customDictionaryPath))
            {
                var newSpeller = new Hunspell(new byte[] {}, File.ReadAllBytes(customDictionaryPath));
                speller.AddHunspell(newSpeller);
            }
        }

        public void AddWordToCustomDictionary(string word)
        {
            var customDictionaryPath = GetCustomDictionaryPath();

            IList<string> lines = File.Exists(customDictionaryPath)
                ? File.ReadAllLines(customDictionaryPath).Skip(1).ToList() // first line is count
                : new List<string>();

            if (!lines.Contains(word))
            {
                lines.Add(word);

                File.WriteAllText(
                    customDictionaryPath,
                    lines.Count() + "\n" + string.Join("\n", lines).TrimEnd('\n') + "\n");

                SetLanguage(currentLanguage); // reloads dictionary
            }
        }

        private static string GetCustomDictionaryPath()
        {
            return Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "custom.dic");
        }
    }
}
