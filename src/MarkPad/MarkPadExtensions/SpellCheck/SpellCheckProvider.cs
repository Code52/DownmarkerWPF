using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MarkPad.Services.Interfaces;
using MarkPad.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.Contracts;
using System.ComponentModel.Composition;

namespace MarkPad.MarkPadExtensions.SpellCheck
{
	[Export(typeof(ISpellCheckProviderFactory))]
	public class SpellCheckProviderFactory : ISpellCheckProviderFactory
	{
		public ISpellCheckProvider GetProvider(ISpellingService spellingService, IDocumentView view)
		{
			return new SpellCheckProvider(spellingService, (DocumentView)view);
		}
	}

    public class SpellCheckProvider : ISpellCheckProvider
    {
        readonly Regex wordSeparatorRegex = new Regex("-[^\\w]+|^'[^\\w]+|[^\\w]+'[^\\w]+|[^\\w]+-[^\\w]+|[^\\w]+'$|[^\\w]+-$|^-$|^'$|[^\\w'-]", RegexOptions.Compiled);
        readonly Regex uriFinderRegex = new Regex("(http|ftp|https|mailto):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?", RegexOptions.Compiled);

        readonly ISpellingService spellingService;
        readonly SpellCheckBackgroundRenderer spellCheckRenderer;

		DocumentView view;
		public IDocumentView View { get { return view; } }

        public SpellCheckProvider(ISpellingService spellingService, DocumentView view)
        {
            this.spellingService = spellingService;
            spellCheckRenderer = new SpellCheckBackgroundRenderer();

            this.view = view;

			this.view.Editor.TextArea.TextView.BackgroundRenderers.Add(spellCheckRenderer);
			this.view.Editor.TextArea.TextView.VisualLinesChanged += TextViewVisualLinesChanged;
        }

        public void Disconnect()
        {
            ClearSpellCheckErrors();
			view.Editor.TextArea.TextView.BackgroundRenderers.Remove(spellCheckRenderer);
			view.Editor.TextArea.TextView.VisualLinesChanged -= TextViewVisualLinesChanged;
        }

        void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            DoSpellCheck();
        }

        private void DoSpellCheck()
        {
			if (!view.Editor.TextArea.TextView.VisualLinesValid) return;

            spellCheckRenderer.ErrorSegments.Clear();

			IEnumerable<VisualLine> visualLines = view.Editor.TextArea.TextView.VisualLines.AsParallel();

            foreach (VisualLine currentLine in visualLines)
            {
                int startIndex = 0;

				string originalText = view.Editor.Document.GetText(currentLine.FirstDocumentLine.Offset, currentLine.LastDocumentLine.EndOffset - currentLine.FirstDocumentLine.Offset);
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
    }

}
