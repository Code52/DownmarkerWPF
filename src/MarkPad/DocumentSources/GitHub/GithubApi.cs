using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MarkPad.Settings.Models;
using RestSharp;
using MarkPad.Infrastructure;

namespace MarkPad.DocumentSources.GitHub
{
    public class GithubApi
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

            return result;
        }

        public async Task<BlogInfo[]> FetchBranches(string token, string user, string repositoryName)
        {
            var client = new RestClient(ApiBaseUrl);
            var result =
                await
                client.ExecuteAwaitableAsync<List<GitBranch>>(
                    new RestRequest(string.Format("/repos/{0}/{1}/branches", user, repositoryName)));


            return result.Data.Select(r => new BlogInfo {blogid = r.name, blogName = r.name}).ToArray();
        }
    }

    public class GitBranch
    {
        public string name { get; set; }
    }
}