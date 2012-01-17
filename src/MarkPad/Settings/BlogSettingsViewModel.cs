using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
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

        public void SetCurrentBlogPassword(object password)
        {
            if (CurrentBlog == null)
                return;

            CurrentBlog.Password = password.ToString();
        }

        public void FetchBlogs()
        {
            this.SelectedAPIBlog = null;

            var proxy = new MetaWeblog(CurrentBlog.WebAPI);

            this.APIBlogs = new ObservableCollection<FetchedBlogInfo>();

            var taskBlogInfo = Task<BlogInfo[]>.Factory.FromAsync(
                                   proxy.BeginGetUsersBlogs,
                                   proxy.EndGetUsersBlogs,
                                   "MarkPad",
                                   CurrentBlog.Username,
                                   CurrentBlog.Password,
                                   null);

            taskBlogInfo.ContinueWith(continueParam =>
            {
                if (continueParam.Exception != null)
                {
                    var message = continueParam.Exception.Message;

                    var aggEx = continueParam.Exception as AggregateException;
                    if (aggEx != null)
                        message = String.Join(Environment.NewLine, aggEx.InnerExceptions.Select(ex => ex.Message));

                    dialogService.ShowError("Markpad", "There was a problem contacting the website. Check the settings and try again.", message);
                    return;
                }

                var newAPIBlogs = new ObservableCollection<FetchedBlogInfo>();

                foreach (var blogInfo in continueParam.Result)
                {
                    newAPIBlogs.Add(new FetchedBlogInfo { Name = blogInfo.blogName, BlogInfo = blogInfo });
                }

                this.APIBlogs = newAPIBlogs;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }

    public class FetchedBlogInfo
    {
        public string Name { get; set; }
        public BlogInfo BlogInfo { get; set; }
    }
}
