using System.IO;
using System.Linq;
using System.Reflection;
using NHunspell;

namespace MarkPad.SpellCheck
{
    public static class DictionariesLoader
    {
        public static void Load(Hunspell speller)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var dictionaryFiles = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(assembly.GetName().Name + ".SpellCheck.Dictionaries."));

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
