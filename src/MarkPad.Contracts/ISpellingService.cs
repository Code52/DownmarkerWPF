using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace MarkPad.Contracts
{
    [InheritedExport]
	public interface ISpellingService
	{
		void ClearLanguages();
		void SetLanguage(SpellingLanguages language);
		bool Spell(string word);
        IEnumerable<string> Suggestions(string word);
	}
}
