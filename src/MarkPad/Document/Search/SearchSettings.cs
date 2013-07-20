namespace MarkPad.Document.Search
{
    public class SearchSettings
    {
        public SearchSettings()
        {
            SearchTerm = string.Empty;
            ReplaceTerm = string.Empty;
        }

        public string SearchTerm { get; set; }

        public string ReplaceTerm { get; set; }

        public bool Regex { get; set; }

        public bool CaseSensitive { get; set; }

        public bool WholeWord { get; set; }

        public bool SelectSearch { get; set; }

        public bool SearchingWithBar { get; set; }
    }
}