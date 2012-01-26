using MarkPad.XAML.Converters;

namespace MarkPad.Services.Interfaces
{
    public enum SpellingLanguages
    {
        [DisplayString("English (Australia)")]
        Australian,
        [DisplayString("English (Canada)")]
        Canadian,
        [DisplayString("English (United States)")]
        UnitedStates
    }

    public interface ISpellingService
    {
        void ClearLanguages();
        void SetLanguage(SpellingLanguages language);

        bool Spell(string word);
    }
}
