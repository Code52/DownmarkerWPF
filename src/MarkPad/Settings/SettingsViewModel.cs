using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;
using Microsoft.Win32;

namespace MarkPad.Settings
{
    public class SettingsViewModel : Screen
    {
        private const string markpadKeyName = "markpad.md";

        private readonly ISettingsService _settingsService;

        public SettingsViewModel(ISettingsService settingsService)
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes"))
            {
                FileMDBinding = key.GetSubKeyNames().Contains(Constants.DefaultExtensions[0]) &&
                    !string.IsNullOrEmpty(key.OpenSubKey(Constants.DefaultExtensions[0]).GetValue("").ToString());

                FileMarkdownBinding = key.GetSubKeyNames().Contains(Constants.DefaultExtensions[1]) &&
                    !string.IsNullOrEmpty(key.OpenSubKey(Constants.DefaultExtensions[1]).GetValue("").ToString());

                FileMDownBinding = key.GetSubKeyNames().Contains(Constants.DefaultExtensions[2]) &&
                    !string.IsNullOrEmpty(key.OpenSubKey(Constants.DefaultExtensions[2]).GetValue("").ToString());
            }

            BlogLanguages = new List<string> { "HTML", "Markdown" };

            _settingsService = settingsService;

            var blogs = _settingsService.Get<List<BlogSetting>>("Blogs");
            if (blogs == null) blogs = new List<BlogSetting>();

            Blogs = new ObservableCollection<BlogSetting>(blogs);
        }

        public bool FileMDBinding { get; set; }
        public bool FileMarkdownBinding { get; set; }
        public bool FileMDownBinding { get; set; }

        public List<string> BlogLanguages { get; set; }

        public string SelectedBlogLanguage
        {
            get
            {
                if (CurrentBlog == null)
                    return "";
                else return CurrentBlog.Language ?? "HTML";
            }
            set { CurrentBlog.Language = value; }
        }

        public BlogSetting CurrentBlog { get; set; }
        public ObservableCollection<BlogSetting> Blogs { get; set; }

        public ObservableCollection<FetchedBlogInfo> APIBlogs { get; set; }
        public FetchedBlogInfo SelectedAPIBlog
        {
            get
            {
                if (CurrentBlog == null)
                    return null;

                else
                {
                    var bi = new FetchedBlogInfo
                                 {
                                     BlogInfo = CurrentBlog.BlogInfo,
                                     Name = CurrentBlog.BlogInfo.blogName
                                 };

                    if (APIBlogs == null) APIBlogs = new ObservableCollection<FetchedBlogInfo>();

                    var listEntry = APIBlogs.SingleOrDefault(b => b.Name == bi.Name);

                    if (listEntry == null)
                    {
                        APIBlogs.Add(bi);
                        return bi;
                    }

                    return listEntry;
                }
            }
            set
            {
                if (CurrentBlog == null) return;
                else
                {
                    if (value == null) CurrentBlog.BlogInfo = new BlogInfo();
                    else CurrentBlog.BlogInfo = value.BlogInfo;
                }
            }
        }

        public void AddBlog()
        {
            var blog = new BlogSetting { BlogName = "New" };
            Blogs.Add(blog);
            CurrentBlog = blog;
        }

        public void RemoveBlog()
        {
            if (CurrentBlog != null)
                Blogs.Remove(CurrentBlog);
        }

        public void FetchBlogs()
        {
            if (string.IsNullOrWhiteSpace(CurrentBlog.WebAPI) ||
                string.IsNullOrWhiteSpace(CurrentBlog.Username) ||
                string.IsNullOrWhiteSpace(CurrentBlog.Password))
            {
                MessageBox.Show("You must enter the API address, Username and Password before fetching blogs.",
                    "Fetch Failed", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            this.SelectedAPIBlog = null;
            try
            {
                var proxy = XmlRpcProxyGen.Create<IMetaWeblog>();
                ((IXmlRpcProxy)proxy).Url = CurrentBlog.WebAPI;

                var blogs = proxy.GetUsersBlogs("MarkPad", CurrentBlog.Username, CurrentBlog.Password);

                this.APIBlogs = new ObservableCollection<FetchedBlogInfo>();

                foreach (var blogInfo in blogs)
                {
                    this.APIBlogs.Add(new FetchedBlogInfo { Name = blogInfo.blogName, BlogInfo = blogInfo });
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message, "Fetch Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Accept()
        {
            foreach (var blog in this.Blogs)
            {
                if (string.IsNullOrWhiteSpace(blog.WebAPI) ||
                    string.IsNullOrWhiteSpace(blog.Username) ||
                    string.IsNullOrWhiteSpace(blog.Password) ||
                    string.IsNullOrWhiteSpace(blog.BlogName) ||
                    string.IsNullOrWhiteSpace(blog.BlogInfo.blogName))
                {
                    MessageBox.Show("You must enter all blog details before saving.",
                                    "Save Failed", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
            }

            UpdateExtensionRegistryKeys();

            _settingsService.Set("Blogs", Blogs.ToList());

            TryClose();
        }

        public void Cancel()
        {
            TryClose();
        }

        private void UpdateExtensionRegistryKeys()
        {
            string exePath = Assembly.GetEntryAssembly().Location;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes", true))
            {
                for (int i = 0; i < Constants.DefaultExtensions.Length; i++)
                {
                    using (RegistryKey extensionKey = key.CreateSubKey(Constants.DefaultExtensions[i]))
                    {
                        if ((i == 0 && FileMDBinding) ||
                            (i == 1 && FileMarkdownBinding) ||
                            (i == 2 && FileMDownBinding))
                            extensionKey.SetValue("", markpadKeyName);
                        else
                            extensionKey.SetValue("", "");
                    }
                }

                using (RegistryKey markpadKey = key.CreateSubKey(markpadKeyName))
                {
                    // Can't get this to work right now.
                    //using (RegistryKey defaultIconKey = markpadKey.CreateSubKey("DefaultIcon"))
                    //{
                    //    defaultIconKey.SetValue("", exePath + ",1");
                    //}

                    using (RegistryKey shellKey = markpadKey.CreateSubKey("shell"))
                    {
                        using (RegistryKey openKey = shellKey.CreateSubKey("open"))
                        {
                            using (RegistryKey commandKey = openKey.CreateSubKey("command"))
                            {
                                commandKey.SetValue("", "\"" + exePath + "\" \"%1\"");
                            }
                        }
                    }
                }
            }
        }
    }

    public class FetchedBlogInfo
    {
        public string Name { get; set; }
        public BlogInfo BlogInfo { get; set; }
    }
}
