using System.ComponentModel;

namespace MarkPad.Document.SpellCheck
{
    public enum SpellingLanguages
    {
        [Description("English (Australia)")]
        Australian,
        [Description("English (Canada)")]
        Canadian,
        [Description("English (United Kingdom)")]
        UnitedKingdom,
        [Description("English (United States)")]
        UnitedStates,
        [Description("German (Germany)")]
        Germany,
        [Description("Spanish (Spain)")]
        Spain,
        [Description("Turkish (Turkey)")]
        Turkish
    }
}
