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
        void SetLanguage(SpellingLanguages language, bool clearOthers = false);

        bool Spell(string word);
    }
}
