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
        private readonly IWindowManager _windowManager;

        public SettingsViewModel(ISettingsService settingsService, IWindowManager windowManager)
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

            _settingsService = settingsService;
            _windowManager = windowManager;

            var blogs = _settingsService.Get<List<BlogSetting>>("Blogs") ?? new List<BlogSetting>();

            Blogs = new ObservableCollection<BlogSetting>(blogs);
        }

        public bool FileMDBinding { get; set; }
        public bool FileMarkdownBinding { get; set; }
        public bool FileMDownBinding { get; set; }

        public BlogSetting CurrentBlog { get; set; }
        public ObservableCollection<BlogSetting> Blogs { get; set; }

        public void AddBlog()
        {
            var blog = new BlogSetting { BlogName = "New", Language = "HTML"};

            blog.BeginEdit();

            var result = _windowManager.ShowDialog(new BlogSettingsViewModel(blog));
            if (result != true)
            {
                blog.CancelEdit();
                return;
            }

            blog.EndEdit();

            Blogs.Add(blog);
        }

        public void EditBlog()
        {
            if (CurrentBlog == null) return;

            CurrentBlog.BeginEdit();

            var result = _windowManager.ShowDialog(new BlogSettingsViewModel(CurrentBlog));

            if (result != true)
            {
                CurrentBlog.CancelEdit();
                return;
            }

            CurrentBlog.EndEdit();
        }

        public void RemoveBlog()
        {
            if (CurrentBlog != null)
                Blogs.Remove(CurrentBlog);
        }

        public void Accept()
        {
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
}
