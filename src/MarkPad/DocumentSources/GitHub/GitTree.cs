using System.Collections.Generic;

namespace MarkPad.DocumentSources.GitHub
{
    public class GitTree
    {
        public GitTree()
        {
            tree = new List<GitFile>();
        }

        public List<GitFile> tree { get; set; }
        public string sha { get; set; }
        public string base_tree { get; set; }
    }
}