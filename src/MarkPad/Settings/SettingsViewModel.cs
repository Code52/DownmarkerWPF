using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using MarkPad.Framework.Events;
using MarkPad.Services.Interfaces;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Media;
using MarkPad.Framework;

namespace MarkPad.Settings
{
    public class SettingsViewModel : Screen
    {
        private const string BlogsSettingsKey = "Blogs";
        private const string DictionariesSettingsKey = "Dictionaries";
        public const string FontSizeSettingsKey = "Font";
		public const string FontFamilySettingsKey = "FontFamily";

        public class ExtensionViewModel : PropertyChangedBase
        {
            public ExtensionViewModel(string extension, bool enabled)
            {
                this.Extension = extension;
                this.Enabled = enabled;
            }

            public string Extension { get; private set; }
            public bool Enabled { get; set; }
        }

        private const string markpadKeyName = "markpad.md";

        private readonly ISettingsService settingsService;
        private readonly IWindowManager windowManager;

		private readonly Func<BlogSettingsViewModel> blogSettingsCreator;
		private readonly Func<FontSelectionViewModel> fontSelectionCreator;

        public SettingsViewModel(ISettingsService settingsService, IWindowManager windowManager, Func<BlogSettingsViewModel> blogSettingsCreator, Func<FontSelectionViewModel> fontSelectionCreator)
        {
            this.settingsService = settingsService;
            this.windowManager = windowManager;
            this.blogSettingsCreator = blogSettingsCreator;
			this.fontSelectionCreator = fontSelectionCreator;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes"))
            {
                this.Extensions = Constants.DefaultExtensions
                    .Select(s => new ExtensionViewModel(s,
                        key.GetSubKeyNames().Contains(s) && !string.IsNullOrEmpty(key.OpenSubKey(s).GetValue("").ToString())))
                    .ToArray();
            }

            var blogs = settingsService.Get<List<BlogSetting>>(BlogsSettingsKey) ?? new List<BlogSetting>();

            Blogs = new ObservableCollection<BlogSetting>(blogs);

            Languages = Enum.GetValues(typeof(SpellingLanguages)).OfType<SpellingLanguages>().ToArray();
            SelectedLanguage = settingsService.Get<SpellingLanguages>(DictionariesSettingsKey);

			SelectedFontSize = settingsService.Get<FontSizes>(FontSizeSettingsKey);
			SelectedFontFamily = Fonts.SystemFontFamilies.First(f => f.Source == settingsService.Get<string>(SettingsViewModel.FontFamilySettingsKey));
        }

        public IEnumerable<ExtensionViewModel> Extensions { get; set; }
		private BlogSetting currentBlog;
		public BlogSetting CurrentBlog
		{
			get { return currentBlog; }
			set
			{
				currentBlog = value;
				this.NotifyOfPropertyChange(() => CanEditBlog);
				this.NotifyOfPropertyChange(() => CanRemoveBlog);
			}
		}
        public ObservableCollection<BlogSetting> Blogs { get; set; }
        public IEnumerable<SpellingLanguages> Languages { get; set; }
        public SpellingLanguages SelectedLanguage { get; set; }
		public FontSizes SelectedFontSize { get; set; }
		public FontFamily SelectedFontFamily { get; set; }

		public int SelectedActualFontSize
		{
			get
			{
				return Constants.FONT_SIZE_ENUM_ADJUSTMENT + (int)SelectedFontSize;
			}
		}
		public string EditorFontPreviewLabel
		{
			get
			{
				return string.Format(
					"Editor font ({0}, {1} pt)",
					SelectedFontFamily.Source,
					SelectedActualFontSize);
			}
		}

        public override string DisplayName
        {
            get { return "Settings"; }
            set { }
        }

        public bool AddBlog()
        {
            var blog = new BlogSetting { BlogName = "New", Language = "HTML" };

            blog.BeginEdit();

            var blogSettings = blogSettingsCreator();
            blogSettings.InitializeBlog(blog);

            var result = windowManager.ShowDialog(blogSettings);
            if (result != true)
            {
                blog.CancelEdit();
                return false;
            }

            blog.EndEdit();

            Blogs.Add(blog);

			return true;
        }

		public bool CanEditBlog { get { return currentBlog != null; } }
        public void EditBlog()
        {
            if (CurrentBlog == null) return;

            CurrentBlog.BeginEdit();

            var blogSettings = blogSettingsCreator();
            blogSettings.InitializeBlog(CurrentBlog);

            var result = windowManager.ShowDialog(blogSettings);

            if (result != true)
            {
                CurrentBlog.CancelEdit();
                return;
            }

            CurrentBlog.EndEdit();
        }

		public bool CanRemoveBlog { get { return currentBlog != null; } }
		public void RemoveBlog()
        {
            if (CurrentBlog != null)
                Blogs.Remove(CurrentBlog);
        }

		public void SelectFont()
		{
			var fontSelection = fontSelectionCreator();
			fontSelection.SelectedFontFamily = SelectedFontFamily;
			fontSelection.SelectedFontSize = SelectedFontSize;

			var result = windowManager.ShowDialog(fontSelection);
			if (result != true) return;

			SelectedFontFamily = fontSelection.SelectedFontFamily;
			SelectedFontSize = fontSelection.SelectedFontSize;
		}

		public void ResetFont()
		{
			SelectedFontFamily = FontHelpers.TryGetFontFamilyFromStack(Constants.DEFAULT_EDITOR_FONT_FAMILY);
			SelectedFontSize = Constants.DEFAULT_EDITOR_FONT_SIZE;
		}

        public void Accept()
        {
            UpdateExtensionRegistryKeys();

            var spellingService = IoC.Get<ISpellingService>();
            spellingService.SetLanguage(SelectedLanguage);

            settingsService.Set(BlogsSettingsKey, Blogs.ToList());
            settingsService.Set(DictionariesSettingsKey, SelectedLanguage);
            settingsService.Set(FontSizeSettingsKey, SelectedFontSize);
			settingsService.Set(FontFamilySettingsKey, SelectedFontFamily.Source);
            settingsService.Save();

            IoC.Get<IEventAggregator>().Publish(new SettingsChangedEvent());

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
                foreach (var ext in Extensions)
                {
                    using (RegistryKey extensionKey = key.CreateSubKey(ext.Extension))
                    {
                        if (ext.Enabled)
                            extensionKey.SetValue("", markpadKeyName);
                        else
                            extensionKey.SetValue("", "");
                    }
                }

                using (RegistryKey markpadKey = key.CreateSubKey(markpadKeyName))
                {
                    using (RegistryKey defaultIconKey = markpadKey.CreateSubKey("DefaultIcon"))
                    {
                        defaultIconKey.SetValue("", Path.Combine(Constants.IconDir, Constants.Icons[0]));
                    }

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
