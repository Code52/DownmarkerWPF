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

        private readonly SearchSettings searchSettings;
        int lastCaretPosition = -1;

        public IEnumerable<TextSegment> SearchHits { get; private set; }
        public int NumberOfHits { get; private set; }
        public int CurrentHitIndex { get; private set; }

#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        public SearchProvider(SearchSettings searchSettings)
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
            if (string.IsNullOrEmpty(searchSettings.SearchTerm)) return;

            var searchTerm = searchSettings.SearchTerm;

            if (!searchSettings.Regex)
            {
                searchTerm = Regex.Escape(searchTerm);
            }
            if (searchSettings.WholeWord)
            {
                searchTerm = @"\b" + searchTerm + @"\b";
            }
            if (!searchSettings.CaseSensitive)
            {
                searchTerm = @"(?i)" + searchTerm;
            }

            foreach (DocumentLine currentDocLine in view.Document.Lines)
            {
                VisualLine currentLine = view.TextView.GetOrConstructVisualLine(currentDocLine);

                string originalText = view.Document.GetText(currentLine.FirstDocumentLine.Offset,
                                                            currentLine.LastDocumentLine.EndOffset -
                                                            currentLine.FirstDocumentLine.Offset);

                try
                {
                    foreach (Match match in Regex.Matches(originalText, searchTerm))
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
            var startLookingFrom = view.Editor.CaretOffset;
            // consider the already selected text when searching, skip it on SearchType.Next
            if (!view.Editor.TextArea.Selection.IsEmpty && searchType != SearchType.Next && searchType != SearchType.Replace)
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

                    newFoundHit = (from hit in SearchHits
                                   let hitDistance = hit.StartOffset - startLookingFrom
                                   where hitDistance < 0
                                   orderby hitDistance descending
                                   select hit)
                                    .FirstOrDefault() ?? SearchHits.Reverse().FirstOrDefault();
                    break;
            }

            // logic for explicit searches
            if (searchType != SearchType.NoSelect)
            {
                newFoundHit.ExecuteSafely(hit =>
                {
                    // special case: don't select text when CTRL+F pressed with an old, existing search, just highlight
                    if (selectSearch)
                    {
                        view.Editor.Select(hit.StartOffset, hit.Length);
                        view.Editor.ScrollToLine(view.Editor.Document.GetLineByOffset(view.Editor.SelectionStart).LineNumber);
                    }

                    lastCaretPosition = view.Editor.CaretOffset;
                    CurrentHitIndex = SearchHits.Select((v, i) => new { hit = v, index = i }).First(arg => arg.hit.Equals(newFoundHit)).index + 1;
                });
            }

            NumberOfHits = searchRenderer.SearchHitsSegments.Count;

            // don't show index of a match if we're searching without a search bar, if there are no matches, or if we're in a NoSelect search and there isn't an already selected old match in the editor
            var selectedText = new TextSegment { StartOffset = view.Editor.SelectionStart, Length = view.Editor.SelectionLength };
            if (!searchSettings.SearchingWithBar || !searchRenderer.SearchHitsSegments.Any() || (!selectSearch && newFoundHit != null && !newFoundHit.EqualsByValue(selectedText)))
            {
                CurrentHitIndex = 0;
            }

            var replaceTerm = searchSettings.ReplaceTerm;

            if (searchType == SearchType.Replace && !view.Editor.TextArea.Selection.IsEmpty)
            {
                view.Editor.TextArea.Selection.ReplaceSelectionWithText(replaceTerm.Trim());

                newFoundHit = (from hit in SearchHits
                               let hitDistance = hit.StartOffset - startLookingFrom
                               where hitDistance >= 0
                               orderby hitDistance
                               select hit)
                              .FirstOrDefault() ?? SearchHits.FirstOrDefault();

                newFoundHit.ExecuteSafely(hit =>
                {
                    // special case: don't select text when CTRL+F pressed with an old, existing search, just highlight
                    if (selectSearch)
                    {
                        view.Editor.Select(hit.StartOffset, hit.Length);
                        view.Editor.ScrollToLine(view.Editor.Document.GetLineByOffset(view.Editor.SelectionStart).LineNumber);
                    }

                    lastCaretPosition = view.Editor.CaretOffset;
                    CurrentHitIndex = SearchHits.Select((v, i) => new { hit = v, index = i }).First(arg => arg.hit.Equals(newFoundHit)).index + 1;
                });
            }

            // don't highlight matches when searching without the search bar
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
