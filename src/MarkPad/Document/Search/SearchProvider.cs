using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using MarkPad.Document.Events;
using MarkPad.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkPad.Document.Search
{
    public class SearchProvider : ISearchProvider
    {
        private readonly SearchBackgroundRenderer searchRenderer;
        private DocumentView view;

        private readonly ISearchSettings searchSettings;
        private SearchType nextSearchType = SearchType.Normal;

        public IEnumerable<TextSegment> SearchHits { get; set; }

        public SearchProvider(ISearchSettings searchSettings)
        {
            this.searchSettings = searchSettings;
            searchRenderer = new SearchBackgroundRenderer();
            SearchHits = searchRenderer.SearchHitsSegments;
        }

        public void Initialise(DocumentView documentView)
        {
            view = documentView;
            view.TextView.BackgroundRenderers.Add(searchRenderer);
            view.TextView.VisualLinesChanged += TextViewVisualLinesChanged;
            DoSearch(SearchType.Normal, false);
        }

        public void Disconnect()
        {
            if (view == null) return;
            ClearSearchHits();
            view.TextView.BackgroundRenderers.Remove(searchRenderer);
            view.TextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            view = null;
        }

        private void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            DoSearchInternal();
        }

        public void DoSearch(SearchType searchType, bool selectSearch = true)
        {
            if (view == null) return;

            nextSearchType = searchType;

            DoSearchInternal(selectSearch);
        }

        private void DoSearchInternal(bool selectSearch = true)
        {
            if (view == null) return;
            if (!view.TextView.VisualLinesValid) return;

            searchRenderer.SearchHitsSegments.Clear();
            if (searchSettings == null) return;
            if (string.IsNullOrEmpty(searchSettings.CurrentSearchTerm)) return;

            var term = searchSettings.CurrentSearchTerm;

            if (!searchSettings.Regex)
            {
                term = Regex.Escape(term);
            }
            if (searchSettings.WholeWord)
            {
                term = @"\b" + term + @"\b";
            }
            if (!searchSettings.CaseSensitive)
            {
                term = @"(?i)" + term;
            }

            foreach (DocumentLine currentDocLine in view.Document.Lines)
            {
                VisualLine currentLine = view.TextView.GetOrConstructVisualLine(currentDocLine);

                string originalText = view.Document.GetText(currentLine.FirstDocumentLine.Offset,
                                                            currentLine.LastDocumentLine.EndOffset -
                                                            currentLine.FirstDocumentLine.Offset);

                try
                {
                    foreach (Match match in Regex.Matches(originalText, term))
                    {
                        var textSegment = new TextSegment
                        {
                            StartOffset = currentLine.FirstDocumentLine.Offset + match.Index,
                            Length = match.Length
                        };

                        searchRenderer.SearchHitsSegments.Add(textSegment);
                    }
                }
                // catch malformed regex
                catch (ArgumentException)
                {
                }
            }

            TextSegment newFoundHit = null;
            var caretOffset = view.Editor.CaretOffset;

            var startLookingFrom = caretOffset;
            if (!view.Editor.TextArea.Selection.IsEmpty && (nextSearchType == SearchType.Normal || nextSearchType == SearchType.Prev))
            {
                startLookingFrom = view.Editor.SelectionStart;
            }

            switch (nextSearchType)
            {
                case SearchType.Normal:
                case SearchType.Next:

                    newFoundHit = (from hit in SearchHits
                                   let fromCaret = hit.StartOffset - startLookingFrom
                                   where fromCaret >= 0
                                   orderby fromCaret
                                   select hit)
                              .FirstOrDefault() ?? SearchHits.FirstOrDefault();
                    break;

                case SearchType.Prev:

                    TextSegment lastHit = (from hit in SearchHits
                                           let fromCaret = hit.StartOffset - startLookingFrom
                                           where fromCaret < 0
                                           orderby fromCaret descending
                                           select hit)
                                            .FirstOrDefault();

                    if (lastHit == SearchHits.LastOrDefault())
                    {
                        newFoundHit = lastHit;
                    }
                    else
                    {
                        newFoundHit = SearchHits.Reverse().SkipWhile(segment => !segment.EqualsByValue(lastHit)).FirstOrDefault()
                                        ?? SearchHits.Reverse().FirstOrDefault(segment => !segment.EqualsByValue(lastHit));
                    }
                    break;
            }

            // logic for explicit searches
            if (nextSearchType != SearchType.NoSelect)
            {
                newFoundHit.ExecuteSafely(hit =>
                {
                    // special case: don't select when CTRL+F pressed with an old, existing search, just highlight
                    if (selectSearch)
                    {
                        view.Editor.Select(hit.StartOffset, hit.Length);
                        view.Editor.ScrollToLine(view.Editor.Document.GetLineByOffset(view.Editor.SelectionStart).LineNumber);
                    }
                });
            }

            // don't highlight when using F3 or SHIFT+F3 without the search bar
            if (!searchSettings.SearchingWithBar)
            {
                searchRenderer.SearchHitsSegments.Clear();
            }

            // don't select text on background searches when visual lines change
            nextSearchType = SearchType.NoSelect;
        }

        private void ClearSearchHits()
        {
            if (searchRenderer == null) return;
            searchRenderer.SearchHitsSegments.Clear();
        }
    }
}