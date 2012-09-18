namespace MarkPad.Document.Search
{
    public class SearchEvent
    {
        public bool SelectSearch { get; private set; }

        public SearchType SearchType { get; private set; }

        public SearchEvent(SearchType searchType, bool selectSearch)
        {
            SelectSearch = selectSearch;
            SearchType = searchType;
        }
    }
}