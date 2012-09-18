using System.ComponentModel;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using MarkPad.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkPad.Document.Search
{
    public class SearchProvider : ISearchProvider, INotifyPropertyChanged
    {
        private readonly SearchBackgroundRenderer searchRenderer;
        private DocumentView view;

        private readonly ISearchSettings searchSettings;
        int lastCaretPosition = -1;

        public IEnumerable<TextSegment> SearchHits { get; private set; }
        public int NumberOfHits { get; private set; }
        public int CurrentHitIndex { get; private set; }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

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
            view.Editor.TextArea.SelectionChanged += TextAreaOnSelectionChanged;
            DoSearch(SearchType.Normal, false);
        }

        public void Disconnect()
        {
            if (view == null) return;
            ClearSearchHits();
            view.TextView.BackgroundRenderers.Remove(searchRenderer);
            view.TextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            view.Editor.TextArea.SelectionChanged -= TextAreaOnSelectionChanged;
            view = null;
        }

        private void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            DoSearch(SearchType.NoSelect, false);
        }

        void TextAreaOnSelectionChanged(object sender, EventArgs eventArgs)
        {
            if (view.Editor.CaretOffset != lastCaretPosition || view.Editor.TextArea.Selection.IsEmpty)
            {
                CurrentHitIndex = 0;
            }
        }

        public void DoSearch(SearchType searchType, bool selectSearch)
        {
            if (view == null) return;
            if (!view.TextView.VisualLinesValid) return;

            ClearSearchHits();
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
                catch (ArgumentException) {} // catch malformed regex
            }

            TextSegment newFoundHit = null;
            var caretOffset = view.Editor.CaretOffset;

            var startLookingFrom = caretOffset;
            if (!view.Editor.TextArea.Selection.IsEmpty && (searchType == SearchType.Normal || searchType == SearchType.Prev))
            {
                startLookingFrom = view.Editor.SelectionStart;
            }

            switch (searchType)
            {
                case SearchType.Normal:
                case SearchType.Next:

                    newFoundHit = (from hit in SearchHits
                                   let hitDistance = hit.StartOffset - startLookingFrom
                                   where hitDistance >= 0
                                   orderby hitDistance
                                   select hit)
                              .FirstOrDefault() ?? SearchHits.FirstOrDefault();
                    break;

                case SearchType.Prev:

                    TextSegment lastHit = (from hit in SearchHits
                                           let hitDistance = hit.StartOffset - startLookingFrom
                                           where hitDistance < 0
                                           orderby hitDistance descending
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
            if (searchType != SearchType.NoSelect)
            {
                newFoundHit.ExecuteSafely(hit =>
                {
                    // special case: don't select when CTRL+F pressed with an old, existing search, just highlight
                    if (selectSearch)
                    {
                        view.Editor.Select(hit.StartOffset, hit.Length);
                        view.Editor.ScrollToLine(view.Editor.Document.GetLineByOffset(view.Editor.SelectionStart).LineNumber);
                    }

                    lastCaretPosition = view.Editor.CaretOffset;
                });
            }

            NumberOfHits = searchRenderer.SearchHitsSegments.Count;

            // get the index of the current match
            // newFoundHit will be null if there are matches, but we are in a SearchType.NoSelect search
            if (newFoundHit != null)
            {
                CurrentHitIndex = SearchHits.Select((v, i) => new {hit = v, index = i}).First(arg => arg.hit.Equals(newFoundHit)).index + 1;
            }

            // don't show index of a match if we're searching without a bar, if there are no matches, or if we're in a no-select search and there is no preselected old match in the editor
            var selectedText = new TextSegment { StartOffset = view.Editor.SelectionStart, Length = view.Editor.SelectionLength };
            if (!searchSettings.SearchingWithBar || !searchRenderer.SearchHitsSegments.Any() || (!selectSearch && newFoundHit != null && !newFoundHit.EqualsByValue(selectedText)))
            {
                CurrentHitIndex = 0;
            }

            // don't highlight when using F3 or SHIFT+F3 without the search bar
            if (!searchSettings.SearchingWithBar)
            {
                ClearSearchHits();
            }
        }

        private void ClearSearchHits()
        {
            if (searchRenderer == null) return;
            searchRenderer.SearchHitsSegments.Clear();
        }
    }
}