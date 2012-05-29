namespace MarkPad.Contracts
{
	public interface ISpellingService
	{
		void ClearLanguages();
		void SetLanguage(SpellingLanguages language);
		bool Spell(string word);
	}
}
