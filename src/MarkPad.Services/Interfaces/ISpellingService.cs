using MarkPad.XAML.Converters;

namespace MarkPad.Services.Interfaces
{
    public enum SpellingLanguages
    {
        Australian,
        Canadian,
        [DisplayString("United States")]
        UnitedStates
    }

    public interface ISpellingService
    {
        void ClearLanguages();
        void SetLanguage(SpellingLanguages language);

        bool Spell(string word);
    }
}
