namespace MarkPad.Document.Search
{
    public interface ISearchSettings
    {
        string CurrentSearchTerm { get; set; }

        string SavedSearchTerm { get; set; }

        bool Regex { get; set; }

        bool CaseSensitive { get; set; }

        bool WholeWord { get; set; }

        bool SelectSearch { get; set; }

        bool SearchingWithBar { get; set; }
    }
}