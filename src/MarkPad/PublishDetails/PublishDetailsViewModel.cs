using Caliburn.Micro;
using MarkPad.Metaweblog;
using MarkPad.Services.Interfaces;
using System.Linq;

namespace MarkPad.PublishDetails
{
    public class PublishDetailsViewModel : Screen
    {
        private Details _post;

        public PublishDetailsViewModel(Details post)
        {
            _post = post;
        }

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
    }

    public class Details
    {
        public string Title { get; set; }
        public string[] Categories { get; set; }
    }
}
