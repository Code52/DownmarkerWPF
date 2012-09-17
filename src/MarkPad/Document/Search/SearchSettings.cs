namespace MarkPad.Document.Search
{
    public class SearchSettings : ISearchSettings
    {
        public SearchSettings()
        {
            CurrentSearchTerm = string.Empty;
        }

        public string CurrentSearchTerm { get; set; }

        public string SavedSearchTerm { get; set; }

        public bool Regex { get; set; }

        public bool CaseSensitive { get; set; }

        public bool WholeWord { get; set; }

        public bool SelectSearch { get; set; }

        public bool SearchingWithBar { get; set; }
    }
}