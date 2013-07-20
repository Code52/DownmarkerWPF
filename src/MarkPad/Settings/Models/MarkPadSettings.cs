using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using MarkPad.Document.SpellCheck;

namespace MarkPad.Settings.Models
{
    public class MarkPadSettings
    {
        public List<string> RecentFiles { get; set; }

        [DefaultValue(FontSizes.FontSize12)]
        public FontSizes FontSize { get; set; }

        [DefaultValue("Segoe UI")]
        public string FontFamily { get; set; }

        public string BlogsJson { get; set; }

		[DefaultValue(true)]
		public bool FloatingToolBarEnabled { get; set; }

        public IndentType IndentType { get; set; }

        [DefaultValue(SpellingLanguages.Australian)]
        public SpellingLanguages Language { get; set; }

        public bool IsEditorColorsInverted { get; set; }

        [DefaultValue(false)]
        public bool MarkdownExtraEnabled { get; set; }
    }
}
