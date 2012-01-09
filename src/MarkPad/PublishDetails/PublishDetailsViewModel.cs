using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using MarkPad.Settings;

namespace MarkPad.PublishDetails
{
    public class PublishDetailsViewModel : Screen
    {
        private readonly Details _post;

        public PublishDetailsViewModel(Details post, List<BlogSetting> blogs)
        {
            _post = post;

            Blogs = new ObservableCollection<BlogSetting>(blogs);
            SelectedBlog = Blogs[0];
        }

        public ObservableCollection<BlogSetting> Blogs { get; set; }

        public string PostTitle
        {
            get { return _post.Title; }
            set { _post.Title = value; }
        }

        public string Categories
        {
            get
            {
                return _post.Categories == null ?  "" : string.Join(",", _post.Categories);
            }
            set { _post.Categories = value.Split(','); }
        }

        public BlogSetting SelectedBlog
        {
            get { return _post.Blog; }
            set { _post.Blog = value; }
        }
    }

    public class Details
    {
        public string Title { get; set; }
        public string[] Categories { get; set; }
        public BlogSetting Blog { get; set; }
    }
}
