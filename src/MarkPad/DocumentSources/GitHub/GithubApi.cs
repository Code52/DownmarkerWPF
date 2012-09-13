using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.GitHub
{
    public class GithubApi : IGithubApi
    {
        public const string ClientId = "ed0cdbf084078f60b8a3";
        private const string ClientSecret = "a0a442815b7530386c90088a98cfd018877624d2";
        public const string RedirectUri = "http://vikingco.de";
        private const string Accesstokenuri = "https://github.com/login/oauth/access_token";
        private const string ApiBaseUrl = "https://api.github.com/";

        public async Task<string> GetToken(string code)
        {
            var c = new HttpClient();
            var data = new Dictionary<string, string>
                           {
                               {"client_id", ClientId},
                               {"client_secret", ClientSecret},
                               {"code", code}
                           };
            var content = new FormUrlEncodedContent(data);
            var request = await c.PostAsync(Accesstokenuri, content);
            var result = await request.Content.ReadAsStringAsync();
            return GetAccessCode(result);
        }

        static string GetAccessCode(string result)
        {
            return HttpUtility.ParseQueryString(result)["access_token"];
        }

        public async Task<BlogInfo[]> FetchBranches(string token, string user, string repositoryName)
        {
            var httpClient = new HttpClient();
            var url = string.Format("/repos/{0}/{1}/branches", user, repositoryName);
            var respose = await httpClient.GetAsync(GetUrl(url, token));
            var result = await respose.Content.ReadAsAsync<List<GitBranch>>();

            return result.Select(r => new BlogInfo { blogid = r.name, blogName = r.name }).ToArray();
        }

        public async Task<Post[]> FetchFiles(string user, string repositoryName, string branch, string token)
        {
            var httpClient = new HttpClient();
            var url = string.Format("/repos/{0}/{1}/branches/{2}", user, repositoryName, branch);
            var respose = await httpClient.GetAsync(GetUrl(url, token));
            var result = await respose.Content.ReadAsAsync<dynamic>();

            var treeUrl = TreeUrl(result);

            respose = await httpClient.GetAsync(new Uri(treeUrl, UriKind.Absolute));
            var tree = await respose.Content.ReadAsAsync<GitTree>();

            return ToPosts(tree);
        }

        static Post[] ToPosts(GitTree tree)
        {
            return tree
                .tree
                .Where(i => i.type == "blob")
                .Select(i => new Post
                {
                    postid = i.sha,
                    title = i.path,
                    permalink = i.url
                })
                .ToArray();
        }

        static string TreeUrl(dynamic deserializeObject)
        {
            var commit = deserializeObject.commit;
            var tree = commit.commit.tree;
            var treeUrl = (string)tree.url;
            return treeUrl;
        }

        public async Task<string> FetchFileContents(string token, string username, string repository, string sha)
        {
            var client = new HttpClient();
            var url = string.Format("/repos/{0}/{1}/git/blobs/{2}", username, repository, sha);
            var restResponse = await client.GetAsync(GetUrl(url, token));

            return await GetContent(restResponse);
        }

        public async Task<Tuple<GitTree, GitCommit>> NewTree(string token, string username, string repository, string branch, GitTree tree)
        {
            var client = new HttpClient();
            //Get base commit ref
            var shaLatestCommit = await GetLatestCommitSha(token, username, repository, branch, client);

            tree.base_tree = await GetBaseTreeSha(token, username, repository, shaLatestCommit, client);

            var gitTree = await GetGitTree(token, username, repository, tree, client);

            var gitCommit = await CreateGitCommit(token, username, repository, shaLatestCommit, gitTree, client);

            LogResult(await FinaliseCommit(token, username, repository, branch, gitCommit.sha, client));

            return Tuple.Create(gitTree, gitCommit);
        }

        void LogResult(HttpResponseMessage httpResponseMessage)
        {
            
        }

        async Task<HttpResponseMessage> FinaliseCommit(string token, string username, string repository, string branch, string shaNewCommit,
                                   HttpClient client)
        {
            var url = string.Format("/repos/{0}/{1}/git/refs/heads/{2}", username, repository, branch);
            return await client.PutAsJsonAsync(GetUrl(url, token), new { sha = shaNewCommit});
        }

        static async Task<GitCommit> CreateGitCommit(string token, string username, string repository, string shaLatestCommit, GitTree gitTree,
                                         HttpClient client)
        {
            var gitCommit = new GitCommit
            {
                message = "Update from Markpad",
                parents = new[] {shaLatestCommit},
                tree = gitTree.sha
            };
            var url = string.Format("/repos/{0}/{1}/git/commits", username, repository);
            var respose = await client.PostAsJsonAsync(GetUrl(url, token), gitCommit);

            var content = await respose.Content.ReadAsAsync<dynamic>();
            gitCommit.sha = content.sha;
            return gitCommit;
        }

        async Task<GitTree> GetGitTree(string token, string username, string repository, GitTree tree, HttpClient client)
        {
            var url = string.Format("/repos/{0}/{1}/git/trees", username, repository);
            var respose = await client.PostAsJsonAsync(GetUrl(url, token), tree);

            return await respose.Content.ReadAsAsync<GitTree>();
        }

        static async Task<string> GetBaseTreeSha(string token, string username, string repository, string shaLatestCommit, HttpClient client)
        {
            var url = string.Format("/repos/{0}/{1}/git/commits/{2}", username, repository, shaLatestCommit);
            var result = await client.GetAsync(GetUrl(url, token));
            var baseTreeResult = await result.Content.ReadAsAsync<dynamic>();
            return baseTreeResult.tree.sha;
        }

        static async Task<string> GetLatestCommitSha(string token, string username, string repository, string branch, HttpClient client)
        {
            var url = string.Format("/repos/{0}/{1}/git/refs/heads/{2}", username, repository, branch);
            var response = await client.GetAsync(GetUrl(url, token));
            var refResult = await response.Content.ReadAsAsync<dynamic>();
            return refResult.@object.sha;
        }

        static async Task<string> GetContent(HttpResponseMessage result)
        {
            var deserializeObject = await result.Content.ReadAsAsync<dynamic>();
            if (deserializeObject.encoding == "utf-8")
                return deserializeObject.content;

            var content = deserializeObject.content;
            string encodedData = content;
            return DecodeFrom64(encodedData);
        }

        public static string DecodeFrom64(string encodedData)
        {
            var encodedDataAsBytes = Convert.FromBase64String(encodedData);

            var returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);

            return returnValue;
        }

        private static string GetUrl(string path, string accessToken)
        {
            return string.Format("{0}/{1}?access_token={2}", ApiBaseUrl.TrimEnd('/'), path.TrimStart('/'), accessToken);
        }
    }
}