using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using MarkPad.Services.Settings;
using MarkPad.Settings.Models;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public class PublishDetailsViewModel : Screen
    {
        private readonly Details post;

        public PublishDetailsViewModel(Details post, List<BlogSetting> blogs)
        {
            this.post = post;

            Blogs = new ObservableCollection<BlogSetting>(blogs);
            SelectedBlog = Blogs[0];
        }

        public ObservableCollection<BlogSetting> Blogs { get; set; }

        public string PostTitle
        {
            get { return post.Title; }
            set { post.Title = value; }
        }

        public string Categories
        {
            get
            {
                return post.Categories == null ? "" : string.Join(",", post.Categories);
            }
            set { post.Categories = value.Split(','); }
        }

        public BlogSetting SelectedBlog
        {
            get { return post.Blog; }
            set { post.Blog = value; }
        }
    }

    public class Details
    {
        public string Title { get; set; }
        public string[] Categories { get; set; }
        public BlogSetting Blog { get; set; }
    }
}
