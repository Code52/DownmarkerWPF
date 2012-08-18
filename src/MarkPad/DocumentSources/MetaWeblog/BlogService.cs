using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using Caliburn.Micro;
using MarkPad.Infrastructure.DialogService;
using MarkPad.Settings;
using MarkPad.Settings.Models;
using MarkPad.Settings.UI;
using Ookii.Dialogs.Wpf;

namespace MarkPad.DocumentSources.MetaWeblog
{
    public class BlogService : IBlogService
    {
        readonly IDialogService dialogService;
        readonly IWindowManager windowManager;
        readonly Func<BlogSettingsViewModel> blogSettingsCreator;
        readonly ISettingsProvider settingsProvider;

        public BlogService(
            IDialogService dialogService,
            IWindowManager windowManager, 
            Func<BlogSettingsViewModel> blogSettingsCreator, 
            ISettingsProvider settingsProvider)
        {
            this.dialogService = dialogService;
            this.windowManager = windowManager;
            this.blogSettingsCreator = blogSettingsCreator;
            this.settingsProvider = settingsProvider;
        }

        public bool ConfigureNewBlog(string featureName)
        {
            var extra = string.Format(
                "The '{0}' feature requires a blog to be configured. A window will be displayed which will allow you to configure a blog.",
                featureName);
            var setupBlog = dialogService.ShowConfirmation(
                "No blogs are configured",
                "Do you want to configure a blog?",
                extra,
                new ButtonExtras(ButtonType.Yes, "Yes", "Configure a blog"),
                new ButtonExtras(ButtonType.No, "No", "Don't configure a blog"));

            if (!setupBlog)
                return false;

            return AddBlog() != null;
        }

        public BlogSetting AddBlog()
        {
            var blog = new BlogSetting { BlogName = "New", Language = "HTML" };

            blog.BeginEdit();

            var blogSettings = blogSettingsCreator();
            blogSettings.InitializeBlog(blog);

            var result = windowManager.ShowDialog(blogSettings);
            if (result != true)
            {
                blog.CancelEdit();
                return null;
            }

            blog.EndEdit();
            var blogs = GetBlogs();
            blogs.Add(blog);
            SaveBlogs(blogs);

            return blog;
        }

        public bool EditBlog(BlogSetting currentBlog)
        {
            var blogs = GetBlogs();
            var blogToUpdate = blogs.SingleOrDefault(b => SameBlog(currentBlog, b));
            currentBlog.BeginEdit();

            var blogSettings = blogSettingsCreator();
            blogSettings.InitializeBlog(currentBlog);

            var result = windowManager.ShowDialog(blogSettings);

            if (result != true)
            {
                currentBlog.CancelEdit();
                return false;
            }

            var index = blogs.IndexOf(blogToUpdate);
            blogs[index] = currentBlog;

            currentBlog.EndEdit();
            SaveBlogs(blogs);
            return true;
        }

        public void Remove(BlogSetting currentBlog)
        {
            var blogs = GetBlogs();
            var blogToRemove = blogs.SingleOrDefault(b => SameBlog(currentBlog, b));

            blogs.Remove(blogToRemove);
            SaveBlogs(blogs);
        }

        static bool SameBlog(BlogSetting b1, BlogSetting b2)
        {
            return b2.BlogInfo.blogName == b1.BlogInfo.blogName && b2.BlogInfo.blogid == b1.BlogInfo.blogid;
        }

        public List<BlogSetting> GetBlogs()
        {
            var settings = settingsProvider.GetSettings<MarkPadSettings>();
            if (string.IsNullOrEmpty(settings.BlogsJson))
                return new List<BlogSetting>();

            var serializer = new DataContractJsonSerializer(typeof (List<BlogSetting>));
            return (List<BlogSetting>)serializer.ReadObject(new MemoryStream(Encoding.Default.GetBytes(settings.BlogsJson)));
        }

        void SaveBlogs(List<BlogSetting> blogs)
        {
            var settings = settingsProvider.GetSettings<MarkPadSettings>();
            var ms = new MemoryStream();
            var writer = JsonReaderWriterFactory.CreateJsonWriter(ms);
            new DataContractJsonSerializer(typeof(List<BlogSetting>)).WriteObject(ms, blogs);
            writer.Flush();
            settings.BlogsJson = Encoding.Default.GetString(ms.ToArray());
            settingsProvider.SaveSettings(settings);
        }
    }
}