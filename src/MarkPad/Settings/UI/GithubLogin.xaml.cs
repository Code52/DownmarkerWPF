using System;
using System.Web;
using System.Windows.Navigation;
using MarkPad.DocumentSources.GitHub;

namespace MarkPad.Settings.UI
{
    public partial class GithubLogin
    {
        

        public GithubLogin()
        {
            InitializeComponent();
            Loaded += OnWbLoginOnLoadCompleted;
            wbLogin.Navigating += WbLoginOnNavigating;

        }

        void WbLoginOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri.ToString().StartsWith("http://vikingco.de"))
            {
                Code = HttpUtility.ParseQueryString(e.Uri.Query)["code"];
                Close();
            }
        }

        public string Code { get; private set; }

        void OnWbLoginOnLoadCompleted(object sender, EventArgs e)
        {
            var url = "https://github.com/login/oauth/authorize?client_id=" + Uri.EscapeDataString(GithubApi.ClientId) + "&redirect_uri=" + Uri.EscapeDataString(GithubApi.RedirectUri) + "&scope=repo&response_type=token";
            var startUri = new Uri(url);

            wbLogin.Navigate(startUri);
        }
    }
}
