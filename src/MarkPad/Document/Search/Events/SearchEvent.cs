namespace MarkPad.Document.Search.Events
{
    public class SearchEvent
    {
        public bool SelectSearch { get; private set; }

        public ISearchSettings SearchSettings { get; private set; }

        public SearchType SearchType { get; private set; }

        public SearchEvent(ISearchSettings searchSettings, SearchType searchType, bool selectSearch)
        {
            SelectSearch = selectSearch;
            SearchSettings = searchSettings;
            SearchType = searchType;
        }
    }
}