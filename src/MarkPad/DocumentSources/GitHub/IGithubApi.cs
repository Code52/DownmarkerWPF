using System;
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
        Task<string> FetchFileContents(string token, string username, string repository, string sha);
        Task<Tuple<GitTree, GitCommit>> NewTree(string token, string username, string repository, string branch, GitTree tree);
    }
}