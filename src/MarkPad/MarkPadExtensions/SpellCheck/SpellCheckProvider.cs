using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MarkPad.Services.Interfaces;
using MarkPad.Document;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;

namespace MarkPad.MarkPadExtensions.SpellCheck
{
	public class SpellCheckProvider
	{
		private readonly Regex WordSeparatorRegex = new Regex("-[^\\w]+|^'[^\\w]+|[^\\w]+'[^\\w]+|[^\\w]+-[^\\w]+|[^\\w]+'$|[^\\w]+-$|^-$|^'$|[^\\w'-]", RegexOptions.Compiled);
		private readonly Regex UriFinderRegex = new Regex("(http|ftp|https|mailto):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?", RegexOptions.Compiled);

		readonly ISpellingService _spellingService;
		readonly SpellCheckBackgroundRenderer _spellCheckRenderer;

		public DocumentView View { get; private set; }

		public SpellCheckProvider(ISpellingService spellingService, DocumentView view)
		{
			_spellingService = spellingService;
			_spellCheckRenderer = new SpellCheckBackgroundRenderer();
			this.View = view;

			this.View.Editor.TextArea.TextView.BackgroundRenderers.Add(_spellCheckRenderer);
			this.View.Editor.TextArea.TextView.VisualLinesChanged += TextView_VisualLinesChanged;
		}

		public void Disconnect()
		{
			ClearSpellCheckErrors();
			this.View.Editor.TextArea.TextView.BackgroundRenderers.Remove(_spellCheckRenderer);
			this.View.Editor.TextArea.TextView.VisualLinesChanged -= TextView_VisualLinesChanged;
		}

		void TextView_VisualLinesChanged(object sender, EventArgs e)
		{
			DoSpellCheck();
		}

		private void DoSpellCheck()
		{
			if (!this.View.Editor.TextArea.TextView.VisualLinesValid) return;

			_spellCheckRenderer.ErrorSegments.Clear();

			IEnumerable<VisualLine> visualLines = this.View.Editor.TextArea.TextView.VisualLines.AsParallel();

			foreach (VisualLine currentLine in visualLines)
			{
				int startIndex = 0;

				string originalText = this.View.Editor.Document.GetText(currentLine.FirstDocumentLine.Offset, currentLine.LastDocumentLine.EndOffset - currentLine.FirstDocumentLine.Offset);
				originalText = Regex.Replace(originalText, "[\\u2018\\u2019\\u201A\\u201B\\u2032\\u2035]", "'");

				var textWithoutURLs = UriFinderRegex.Replace(originalText, "");

				var query = WordSeparatorRegex.Split(textWithoutURLs)
					.Where(s => !string.IsNullOrEmpty(s));

				foreach (var word in query)
				{
					string trimmedWord = word.Trim('\'', '_', '-');

					int num = currentLine.FirstDocumentLine.Offset + originalText.IndexOf(trimmedWord, startIndex);

					if (!_spellingService.Spell(trimmedWord))
					{
						var textSegment = new TextSegment
						{
							StartOffset = num,
							Length = word.Length
						};
						_spellCheckRenderer.ErrorSegments.Add(textSegment);
					}

					startIndex = originalText.IndexOf(word, startIndex) + word.Length;
				}
			}
		}

		private void ClearSpellCheckErrors()
		{
			_spellCheckRenderer.ErrorSegments.Clear();
		}
	}

}
