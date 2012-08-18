using System.Collections.Generic;

namespace MarkPad.Document.SpellCheck
{
	public interface ISpellingService
	{
		void ClearLanguages();
		void SetLanguage(SpellingLanguages language);
		bool Spell(string word);
        IEnumerable<string> Suggestions(string word);
	}
}
