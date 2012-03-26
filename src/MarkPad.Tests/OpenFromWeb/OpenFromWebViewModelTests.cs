using System.Collections.Generic;
using MarkPad.OpenFromWeb;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Settings;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.OpenFromWeb
{
    public class OpenFromWebViewModelTests
    {
        OpenFromWebViewModel subject;
        
        readonly IMetaWeblogService metaWeblogService = Substitute.For<IMetaWeblogService>();
        readonly IDialogService dialogService = Substitute.For<IDialogService>();

        public OpenFromWebViewModelTests()
        {
            subject = new OpenFromWebViewModel(dialogService, s => metaWeblogService);
        }

        [Fact]
        public void InitializeBlogs_WithNoBlogs_HasNoData()
        {
            subject.InitializeBlogs(new List<BlogSetting>());

            Assert.Empty(subject.Blogs);
            Assert.Null(subject.SelectedBlog);
        }

        [Fact]
        public void CanFetch_WithNoBlogs_ReturnsFalse()
        {
            subject.InitializeBlogs(new List<BlogSetting>());

            Assert.False(subject.CanFetch);
        }

        [Fact]
        public void CanFetch_WithOneOrMoreBlogs_ReturnsTrue()
        {
            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            Assert.True(subject.CanFetch);
        }
    }
}
