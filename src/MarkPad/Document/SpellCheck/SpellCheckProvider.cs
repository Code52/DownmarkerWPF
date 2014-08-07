using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;

namespace MarkPad.Document.SpellCheck
{
    public class SpellCheckProvider : ISpellCheckProvider
    {
        readonly Regex wordSeparatorRegex = new Regex("-[^\\w]+|^'[^\\w]+|[^\\w]+'[^\\w]+|[^\\w]+-[^\\w]+|[^\\w]+'$|[^\\w]+-$|^-$|^'$|[^\\w'-]", RegexOptions.Compiled);
        readonly Regex uriFinderRegex = new Regex("(http|ftp|https|mailto):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?", RegexOptions.Compiled);

        readonly ISpellingService spellingService;
        readonly SpellCheckBackgroundRenderer spellCheckRenderer;
        DocumentView view;

        public SpellCheckProvider(ISpellingService spellingService)
        {
            this.spellingService = spellingService;
            spellCheckRenderer = new SpellCheckBackgroundRenderer();
        }

        public void Initialise(DocumentView documentView)
        {
            view = documentView;
            view.TextView.BackgroundRenderers.Add(spellCheckRenderer);
            view.TextView.VisualLinesChanged += TextViewVisualLinesChanged;
        }

        public void Disconnect()
        {
            if (view == null) return;
            ClearSpellCheckErrors();
            view.TextView.BackgroundRenderers.Remove(spellCheckRenderer);
            view.TextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            view = null;
        }

        void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            DoSpellCheck();
        }

        private void DoSpellCheck()
        {
            if (view == null) return;
            if (!view.TextView.VisualLinesValid) return;

            spellCheckRenderer.ErrorSegments.Clear();

            IEnumerable<VisualLine> visualLines = view.TextView.VisualLines.AsParallel();

            foreach (VisualLine currentLine in visualLines)
            {
                int startIndex = 0;

                string originalText = view.Document.GetText(currentLine.FirstDocumentLine.Offset, currentLine.LastDocumentLine.EndOffset - currentLine.FirstDocumentLine.Offset);
                originalText = Regex.Replace(originalText, "[\\u2018\\u2019\\u201A\\u201B\\u2032\\u2035]", "'");

                var textWithoutUrls = uriFinderRegex.Replace(originalText, "");

                var query = wordSeparatorRegex.Split(textWithoutUrls)
                    .Where(s => !string.IsNullOrEmpty(s));

                foreach (var word in query)
                {
                    string trimmedWord = word.Trim('\'', '_', '-');

                    int num = currentLine.FirstDocumentLine.Offset + originalText.IndexOf(trimmedWord, startIndex, StringComparison.InvariantCultureIgnoreCase);

                    if (!spellingService.Spell(trimmedWord))
                    {
                        var textSegment = new TextSegment
                        {
                            StartOffset = num,
                            Length = word.Length
                        };
                        spellCheckRenderer.ErrorSegments.Add(textSegment);
                    }

                    startIndex = originalText.IndexOf(word, startIndex, StringComparison.InvariantCultureIgnoreCase) + word.Length;
                }
            }
        }

        private void ClearSpellCheckErrors()
        {
            if (spellCheckRenderer == null) return;
            spellCheckRenderer.ErrorSegments.Clear();
        }

        public IEnumerable<TextSegment> GetSpellCheckErrors()
        {
            if (spellCheckRenderer == null) return Enumerable.Empty<TextSegment>();
            return spellCheckRenderer.ErrorSegments;
        }

        public IEnumerable<string> GetSpellcheckSuggestions(string word)
        {
            if (spellCheckRenderer == null) return Enumerable.Empty<string>();
            return spellingService.Suggestions(word);
        }

        public void AddWordToCustomDictionary(string word)
        {
            spellingService.AddWordToCustomDictionary(word);
            DoSpellCheck();
        }
    }
}
