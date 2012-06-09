using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using MarkPad.DocumentSources.MetaWeblog.Service;
using MarkPad.OpenFromWeb;
using MarkPad.Services.Interfaces;
using MarkPad.Services.Settings;
using NSubstitute;
using Xunit;

namespace MarkPad.Tests.OpenFromWeb
{
    public class OpenFromWebViewModelTests
    {
        readonly OpenFromWebViewModel subject;
        
        readonly IMetaWeblogService metaWeblogService = Substitute.For<IMetaWeblogService>();
        readonly IDialogService dialogService = Substitute.For<IDialogService>();
        readonly ITaskSchedulerFactory taskScheduler = Substitute.For<ITaskSchedulerFactory>();

        public OpenFromWebViewModelTests()
        {
            taskScheduler.FromCurrentSynchronisationContext().Returns(TaskScheduler.Default);

            subject = new OpenFromWebViewModel(dialogService, s => metaWeblogService, taskScheduler);
        }

        [Fact]
        public void InitializeBlogs_WithNoBlogs_HasNoData()
        {
            subject.InitializeBlogs(new List<BlogSetting>());

            Assert.Empty(subject.Blogs);
            Assert.Null(subject.SelectedBlog);
        }

        [Fact]
        public void Cancel_Always_ClosesViewModel()
        {
            var conductor = Substitute.For<IConductor>();
            subject.Parent = conductor;

            subject.Cancel();

            // this is slightly different to how the implementation is
            // but without access to the Views collections 
            // we cannot emulate the dialog result
            conductor.Received().CloseItem(subject);
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

        [Fact]
        public void CanContinue_AfterFetchingNoPosts_ReturnsFalse()
        {
            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => new Post[0]));

            subject.Fetch()
                .Wait();

            Assert.Empty(subject.Posts);
            Assert.False(subject.CanContinue);
        }

        [Fact]
        public void Fetch_ThrowingException_RaisesDialog()
        {
            subject.InitializeBlogs(new List<BlogSetting> {new BlogSetting()});

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => { throw new Exception(); } ));

            subject.Fetch()
                .Wait();

            dialogService.Received().ShowError("Markpad", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public void CurrentPost_AfterFetchingOnePost_SelectsFirstPost()
        {
            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            var posts = new[] {new Post { title = "ABC"}};

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => posts));

            subject.Fetch()
                .Wait();

            Assert.NotEmpty(subject.Posts);
            Assert.Equal("ABC", subject.CurrentPost.Key);
        }

        [Fact]
        public void CanContinue_AfterFetchingOnePost_ReturnsTrue()
        {
            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            var posts = new[] { new Post { title = "ABC" } };

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => posts));

            subject.Fetch()
                .Wait();

            Assert.NotEmpty(subject.Posts);
            Assert.True(subject.CanContinue);
        }

        [Fact]
        public void Continue_WhenPostSelected_ClosesViewModel()
        {
            var conductor = Substitute.For<IConductor>();
            subject.Parent = conductor;

            subject.InitializeBlogs(new List<BlogSetting> { new BlogSetting() });

            var posts = new[] { new Post { title = "ABC" } };

            metaWeblogService
                .GetRecentPostsAsync(Arg.Any<BlogSetting>(), Arg.Any<int>())
                .Returns(Task<Post[]>.Factory.StartNew(() => posts));

            subject.Fetch()
                .Wait();

            subject.Continue();

            // this is different to how the class is used
            // but without access to the Views collections 
            // we cannot emulate the dialog result behaviour perfectly
            conductor.Received().CloseItem(subject);
        }
    }
}
