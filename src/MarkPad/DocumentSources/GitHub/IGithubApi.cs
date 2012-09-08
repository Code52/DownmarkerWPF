using System.Threading.Tasks;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.GitHub
{
    public interface IGithubApi
    {
        Task<string> GetToken(string code);
        Task<BlogInfo[]> FetchBranches(string token, string user, string repositoryName);
        Task<Post[]> FetchFiles(string username, string repository, string branch, string token);
        Task<Post> FetchFileContents(string token, Post selectedPost);
        Task<GitTree> NewTree(string token, string username, string repository, GitTree tree);
    }
}