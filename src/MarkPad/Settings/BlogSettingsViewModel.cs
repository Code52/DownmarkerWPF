using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Caliburn.Micro;
using CookComputing.XmlRpc;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;

namespace MarkPad.Settings
{
    public class BlogSettingsViewModel : Screen
    {
        private readonly IDialogService dialogService;

        public BlogSettingsViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            BlogLanguages = new List<string> { "HTML", "Markdown" };
        }

        public override string DisplayName
        {
            get { return "Blog Settings"; }
            set { }
        }

        public void InitializeBlog(BlogSetting blog)
        {
            CurrentBlog = blog;
        }

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

        public void FetchBlogs()
        {
            this.SelectedAPIBlog = null;
            try
            {
                var proxy = new MetaWeblog(CurrentBlog.WebAPI);

                var taskBlogInfo = Task<BlogInfo[]>.Factory.FromAsync(
                                       proxy.BeginGetUsersBlogs,
                                       proxy.EndGetUsersBlogs,
                                       "MarkPad",
                                       CurrentBlog.Username,
                                       CurrentBlog.Password,
                                       null);
                taskBlogInfo.ContinueWith(continueParam =>
                {
                    this.APIBlogs = new ObservableCollection<FetchedBlogInfo>();

                    foreach (var blogInfo in continueParam.Result)
                    {
                        this.APIBlogs.Add(new FetchedBlogInfo { Name = blogInfo.blogName, BlogInfo = blogInfo });
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (WebException ex)
            {
                dialogService.ShowError("Fetch Failed", ex.Message, "");
            }
            catch (XmlRpcException ex)
            {
                dialogService.ShowError("Fetch Failed", ex.Message, "");
            }
            catch (XmlRpcFaultException ex)
            {
                dialogService.ShowError("Fetch Failed", ex.Message, "");
            }
        }
    }

    public class FetchedBlogInfo
    {
        public string Name { get; set; }
        public BlogInfo BlogInfo { get; set; }
    }
}
