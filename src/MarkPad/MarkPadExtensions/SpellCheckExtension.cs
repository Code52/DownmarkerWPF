using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.Document;
using MarkPad.Services.Interfaces;
using System.Text.RegularExpressions;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Document;

namespace MarkPad.MarkPadExtensions
{
	public class SpellCheckExtension : IDocumentViewExtension
	{
		private readonly Regex WordSeparatorRegex = new Regex("-[^\\w]+|^'[^\\w]+|[^\\w]+'[^\\w]+|[^\\w]+-[^\\w]+|[^\\w]+'$|[^\\w]+-$|^-$|^'$|[^\\w'-]", RegexOptions.Compiled);
		private readonly Regex UriFinderRegex = new Regex("(http|ftp|https|mailto):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?", RegexOptions.Compiled);

		readonly ISpellingService _spellingService;
		readonly SpellCheckBackgroundRenderer _spellCheckRenderer;

		public string Name { get { return "Spell check"; } }

		DocumentView _view;

		public SpellCheckExtension(ISpellingService spellingService)
		{
			_spellingService = spellingService;
			_spellCheckRenderer = new SpellCheckBackgroundRenderer();
		}

		public void ConnectToDocumentView(DocumentView view)
		{
			_view = view;

			_view.Editor.TextArea.TextView.BackgroundRenderers.Add(_spellCheckRenderer);
			_view.Editor.TextArea.TextView.VisualLinesChanged += TextView_VisualLinesChanged;
		}

		public void DisconnectFromDocumentView()
		{
			ClearSpellCheckErrors();
			_view.Editor.TextArea.TextView.BackgroundRenderers.Remove(_spellCheckRenderer);
			_view.Editor.TextArea.TextView.VisualLinesChanged -= TextView_VisualLinesChanged; 
		}

		void TextView_VisualLinesChanged(object sender, EventArgs e)
		{
			DoSpellCheck();
		}

		private void DoSpellCheck()
		{
			if (!_view.Editor.TextArea.TextView.VisualLinesValid) return;

			_spellCheckRenderer.ErrorSegments.Clear();

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
