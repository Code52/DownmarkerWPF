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
        private TextSegment lastHit;
        private int lastCaretPosition;

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
            bool dontSkipPreviousMatch = false;

            if (lastHit == null || (lastCaretPosition != caretOffset && nextSearchType != SearchType.Normal))
            {
                lastHit = (from hit in SearchHits
                            let fromCaret = hit.StartOffset - caretOffset
                            where fromCaret < 0
                            orderby fromCaret descending
                            select hit)
                            .FirstOrDefault();

                dontSkipPreviousMatch = true;
            }

            switch (nextSearchType)
            {
                case SearchType.Normal:

                    var startLookingFrom = view.Editor.TextArea.Selection.IsEmpty
                                               ? caretOffset
                                               : view.Editor.SelectionStart;

                    // search from startLookingFrom to eof, start from bof if no matches
                    newFoundHit = (from hit in SearchHits
                                   let fromCaret = hit.StartOffset - startLookingFrom
                                   where fromCaret >= 0
                                   orderby fromCaret
                                   select hit)
                                      .FirstOrDefault() ?? SearchHits.FirstOrDefault();
                    break;

                case SearchType.Next:

                    if (lastHit == null) // caret was moved, no matches before caret position
                    {
                        newFoundHit = SearchHits.FirstOrDefault();
                    }
                    else
                    {
                        // try to find first match, starting from the position of the last match
                        newFoundHit = SearchHits.SkipWhile(segment => !segment.EqualsByValue(lastHit)).Skip(1).FirstOrDefault();

                        // start from the bof downwards if no hits from last->eof
                        if (newFoundHit == null)
                        {
                            newFoundHit = SearchHits.FirstOrDefault(segment => !segment.EqualsByValue(lastHit));
                        }
                    }
                    break;

                case SearchType.Prev:

                    if (lastHit == null) // caret was moved, no matches before caret position
                    {
                        newFoundHit = SearchHits.Reverse().FirstOrDefault();
                    }
                    else
                    {
                        // try to find first match, starting from the position of the last match
                        // use lastHit value if it was refreshed due to caret move
                        var fromLastHit = SearchHits.Reverse().SkipWhile(segment => !segment.EqualsByValue(lastHit));
                        newFoundHit = dontSkipPreviousMatch ? fromLastHit.FirstOrDefault() : fromLastHit.Skip(1).FirstOrDefault();

                        // start from the eof upwards if no hits from last->bof
                        if (newFoundHit == null)
                        {
                            newFoundHit = SearchHits.Reverse().FirstOrDefault(segment => !segment.EqualsByValue(lastHit));
                        }
                    }
                    break;
            }

            // logic for explicit searches
            if (nextSearchType != SearchType.NoSelect)
            {
                newFoundHit.ExecuteSafely(hit =>
                {
                    // special case: don't select or update lastHit when CTRL+F pressed with an old, existing search, just highlight
                    if (selectSearch)
                    {
                        view.Editor.Select(hit.StartOffset, hit.Length);
                        view.Editor.ScrollToLine(view.Editor.Document.GetLineByOffset(view.Editor.SelectionStart).LineNumber);
                        lastHit = hit;
                    }
                    else
                    {
                        lastHit = null;
                    }

                    lastCaretPosition = view.Editor.CaretOffset;
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