using ICSharpCode.AvalonEdit.Document;
using MarkPad.Document.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkPad.Document.Search
{
    public interface ISearchProvider
    {
        void Initialise(DocumentView documentView);

        void Disconnect();

        IEnumerable<TextSegment> SearchHits { get; set; }

        void DoSearch(SearchType searchType, bool selectSearch = true);
    }
}