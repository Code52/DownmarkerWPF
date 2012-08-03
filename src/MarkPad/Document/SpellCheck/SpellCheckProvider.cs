using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.Contracts;

namespace MarkPad.Document.SpellCheck
{
    public class SpellCheckProvider : ISpellCheckProvider
    {
        readonly Regex wordSeparatorRegex = new Regex("-[^\\w]+|^'[^\\w]+|[^\\w]+'[^\\w]+|[^\\w]+-[^\\w]+|[^\\w]+'$|[^\\w]+-$|^-$|^'$|[^\\w'-]", RegexOptions.Compiled);
        readonly Regex uriFinderRegex = new Regex("(http|ftp|https|mailto):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?", RegexOptions.Compiled);

        readonly ISpellingService spellingService;
        readonly SpellCheckBackgroundRenderer spellCheckRenderer;

        public IDocumentView View { get; private set; }

        public SpellCheckProvider(ISpellingService spellingService, IDocumentView view)
        {
            this.spellingService = spellingService;
            spellCheckRenderer = new SpellCheckBackgroundRenderer();

            View = view;

			View.TextView.BackgroundRenderers.Add(spellCheckRenderer);
			View.TextView.VisualLinesChanged += TextViewVisualLinesChanged;
        }

        public void Disconnect()
        {
            ClearSpellCheckErrors();
			View.TextView.BackgroundRenderers.Remove(spellCheckRenderer);
			View.TextView.VisualLinesChanged -= TextViewVisualLinesChanged;
        }

        void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            DoSpellCheck();
        }

        private void DoSpellCheck()
        {
			if (!View.TextView.VisualLinesValid) return;

            spellCheckRenderer.ErrorSegments.Clear();

			IEnumerable<VisualLine> visualLines = View.TextView.VisualLines.AsParallel();

            foreach (VisualLine currentLine in visualLines)
            {
                int startIndex = 0;

				string originalText = View.Document.GetText(currentLine.FirstDocumentLine.Offset, currentLine.LastDocumentLine.EndOffset - currentLine.FirstDocumentLine.Offset);
                originalText = Regex.Replace(originalText, "[\\u2018\\u2019\\u201A\\u201B\\u2032\\u2035]", "'");

                var textWithoutURLs = uriFinderRegex.Replace(originalText, "");

                var query = wordSeparatorRegex.Split(textWithoutURLs)
                    .Where(s => !string.IsNullOrEmpty(s));

                foreach (var word in query)
                {
                    string trimmedWord = word.Trim('\'', '_', '-');

                    int num = currentLine.FirstDocumentLine.Offset + originalText.IndexOf(trimmedWord, startIndex);

                    if (!spellingService.Spell(trimmedWord))
                    {
                        var textSegment = new TextSegment
                        {
                            StartOffset = num,
                            Length = word.Length
                        };
                        spellCheckRenderer.ErrorSegments.Add(textSegment);
                    }

                    startIndex = originalText.IndexOf(word, startIndex) + word.Length;
                }
            }
        }

        private void ClearSpellCheckErrors()
        {
            spellCheckRenderer.ErrorSegments.Clear();
        }

        public IEnumerable<TextSegment> GetSpellCheckErrors()
        {
            return spellCheckRenderer.ErrorSegments;
        }

        public IEnumerable<string> GetSpellcheckSuggestions(string word)
        {
            return spellingService.Suggestions(word);
        } 
    }

}
