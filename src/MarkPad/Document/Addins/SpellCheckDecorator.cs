using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Rendering;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Document;
using MarkPad.Services.Interfaces;

namespace MarkPad.Document.Addins
{
	public class SpellCheckAddin : IDocumentViewAddin
	{
        private readonly ISpellingService spellingService;
        private readonly SpellCheckBackgroundRenderer spellCheckRenderer;

        private readonly Regex UriFinderRegex = new Regex("(http|ftp|https|mailto):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?", RegexOptions.Compiled);
        private readonly Regex WordSeparatorRegex = new Regex("-[^\\w]+|^'[^\\w]+|[^\\w]+'[^\\w]+|[^\\w]+-[^\\w]+|[^\\w]+'$|[^\\w]+-$|^-$|^'$|[^\\w'-]", RegexOptions.Compiled);

		DocumentView _view;

		public SpellCheckAddin(ISpellingService spellingService)
		{
			this.spellingService = spellingService;
			spellCheckRenderer = new SpellCheckBackgroundRenderer();
		}

		public void ConnectTo(DocumentView view)
		{
			_view = view;

			_view.Editor.TextArea.TextView.BackgroundRenderers.Add(spellCheckRenderer);
			_view.Editor.TextArea.TextView.VisualLinesChanged += TextView_VisualLinesChanged;
		}

		public void Disconnect()
		{
			ClearSpellCheckErrors();
			_view.Editor.TextArea.TextView.BackgroundRenderers.Remove(spellCheckRenderer);
			_view.Editor.TextArea.TextView.VisualLinesChanged -= TextView_VisualLinesChanged;
		}

        void TextView_VisualLinesChanged(object sender, EventArgs e)
        {
            DoSpellCheck();
        }		

		private void DoSpellCheck()
		{
			if (!_view.Editor.TextArea.TextView.VisualLinesValid)
				return;

			this.spellCheckRenderer.ErrorSegments.Clear();

			IEnumerable<VisualLine> visualLines = _view.Editor.TextArea.TextView.VisualLines.AsParallel();

			foreach (VisualLine currentLine in visualLines)
			{
				int startIndex = 0;

				string originalText = _view.Editor.Document.GetText(currentLine.FirstDocumentLine.Offset, currentLine.LastDocumentLine.EndOffset - currentLine.FirstDocumentLine.Offset);
				originalText = Regex.Replace(originalText, "[\\u2018\\u2019\\u201A\\u201B\\u2032\\u2035]", "'");

				var textWithoutURLs = UriFinderRegex.Replace(originalText, "");

				var query = WordSeparatorRegex.Split(textWithoutURLs)
					.Where(s => !string.IsNullOrEmpty(s));

				foreach (var word in query)
				{
					string trimmedWord = word.Trim('\'', '_', '-');

					int num = currentLine.FirstDocumentLine.Offset + originalText.IndexOf(trimmedWord, startIndex);

					if (!spellingService.Spell(trimmedWord))
					{
						TextSegment textSegment = new TextSegment();
						textSegment.StartOffset = num;
						textSegment.Length = word.Length;
						this.spellCheckRenderer.ErrorSegments.Add(textSegment);
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
