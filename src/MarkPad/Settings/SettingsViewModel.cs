using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Caliburn.Micro;
using MarkPad.Framework;
using MarkPad.Framework.Events;
using MarkPad.Plugins;
using MarkPad.Services;
using MarkPad.Services.Implementation;
using MarkPad.Services.Settings;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using MarkPad.Contracts;

namespace MarkPad.Settings
{
    public class SettingsViewModel : Screen
    {
        public const string FontSizeSettingsKey = "Font";
        public const string FontFamilySettingsKey = "FontFamily";
        private const string MarkpadKeyName = "markpad.md";

        private readonly ISettingsProvider settingsService;
        private readonly IWindowManager windowManager;
        private readonly IEventAggregator eventAggregator;
        private readonly Func<BlogSettingsViewModel> blogSettingsCreator;
		private readonly IPluginManager pluginManager;
		private readonly Func<IPlugin, PluginViewModel> pluginViewModelCreator;
		private readonly ISpellingService spellingService;

        public IEnumerable<ExtensionViewModel> Extensions { get; set; }
        public IEnumerable<FontSizes> FontSizes { get; set; }
        public IEnumerable<FontFamily> FontFamilies { get; set; }
        public ObservableCollection<BlogSetting> Blogs { get; set; }
        public IEnumerable<SpellingLanguages> Languages { get; set; }
        public SpellingLanguages SelectedLanguage { get; set; }
        public FontSizes SelectedFontSize { get; set; }
        public FontFamily SelectedFontFamily { get; set; }
		public bool EnableFloatingToolBar { get; set; }
		public PluginViewModel SelectedPlugin { get; set; }
		public IEnumerable<PluginViewModel> Plugins { get; private set; }

		[ImportMany]
		IEnumerable<IPlugin> plugins;

        public SettingsViewModel(
            ISettingsProvider settingsService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            Func<BlogSettingsViewModel> blogSettingsCreator,
			IPluginManager pluginManager,
			Func<IPlugin, PluginViewModel> pluginViewModelCreator,
			ISpellingService spellingService)
        {
            this.settingsService = settingsService;
            this.windowManager = windowManager;
            this.eventAggregator = eventAggregator;
            this.blogSettingsCreator = blogSettingsCreator;
			this.pluginManager = pluginManager;
			this.pluginViewModelCreator = pluginViewModelCreator;
			this.spellingService = spellingService;

			this.pluginManager.Container.ComposeParts(this);
        }

        public void Initialize()
        {
            using (var key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes"))
            {
                Extensions = Constants.DefaultExtensions
                    .Select(s => new ExtensionViewModel(s,
                        key.GetSubKeyNames().Contains(s) && !string.IsNullOrEmpty(key.OpenSubKey(s).GetValue("").ToString())))
                    .ToArray();
            }
            
            var settings = settingsService.GetSettings<MarkPadSettings>();
            var blogs = settings.GetBlogs();

            Blogs = new ObservableCollection<BlogSetting>(blogs);

            Languages = Enum.GetValues(typeof(SpellingLanguages)).OfType<SpellingLanguages>().ToArray();
            FontSizes = Enum.GetValues(typeof(FontSizes)).OfType<FontSizes>().ToArray();
            FontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);

            var spellCheckPlugin = plugins.OfType<SpellCheckPlugin.SpellCheckPluginSettings>().FirstOrDefault();

            if (spellCheckPlugin != null)
            {
                SelectedLanguage = spellCheckPlugin.Language;    
            }
            else
            {
                SelectedLanguage = SpellingLanguages.UnitedStates;
            }

            var fontFamily = settings.FontFamily;
            SelectedFontFamily = Fonts.SystemFontFamilies.FirstOrDefault(f => f.Source == fontFamily);
            SelectedFontSize = settings.FontSize;

            if (SelectedFontFamily == null)
            {
                SelectedFontFamily = FontHelpers.TryGetFontFamilyFromStack(Constants.DEFAULT_EDITOR_FONT_FAMILY);
                SelectedFontSize = Constants.DEFAULT_EDITOR_FONT_SIZE;
            }
			EnableFloatingToolBar = settings.FloatingToolBarEnabled;

			Plugins = plugins
				.Where(plugin => !plugin.IsHidden)
				.Select(plugin => pluginViewModelCreator(plugin));
        }

        private BlogSetting currentBlog;
        public BlogSetting CurrentBlog
        {
            get { return currentBlog; }
            set
            {
                currentBlog = value;
                NotifyOfPropertyChange(() => CanEditBlog);
                NotifyOfPropertyChange(() => CanRemoveBlog);
            }
        }

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

        public void ResetFont()
        {
            SelectedFontFamily = FontHelpers.TryGetFontFamilyFromStack(Constants.DEFAULT_EDITOR_FONT_FAMILY);
            SelectedFontSize = Constants.DEFAULT_EDITOR_FONT_SIZE;
        }

        public void Accept()
        {
            UpdateExtensionRegistryKeys();

            spellingService.SetLanguage(SelectedLanguage);

            var settings = settingsService.GetSettings<MarkPadSettings>();

            settings.SaveBlogs(Blogs.ToList());
            settings.FontSize = SelectedFontSize;
            settings.FontFamily = SelectedFontFamily.Source;
			settings.FloatingToolBarEnabled = EnableFloatingToolBar;
			
            settingsService.SaveSettings(settings);

			// TODO: Move to per-plugin setting screen
            var spellCheckPluginSettings = plugins.OfType<SpellCheckPlugin.SpellCheckPluginSettings>().FirstOrDefault();

            if (spellCheckPluginSettings != null)
            {
                spellCheckPluginSettings.Language = SelectedLanguage;
                settingsService.SaveSettings(spellCheckPluginSettings);
            }
			
            eventAggregator.Publish(new SettingsChangedEvent());
        }

		public void HideSettings()
		{
			eventAggregator.Publish(new SettingsCloseEvent());
			Accept();
		}

        private void UpdateExtensionRegistryKeys()
        {
            var exePath = Assembly.GetEntryAssembly().Location;

            using (var key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Classes", true))
            {
                foreach (var ext in Extensions)
                {
                    using (var extensionKey = key.CreateSubKey(ext.Extension))
                    {
                        extensionKey.SetValue("", ext.Enabled ? MarkpadKeyName : "");
                    }
                }

                using (var markpadKey = key.CreateSubKey(MarkpadKeyName))
                {
                    using (var defaultIconKey = markpadKey.CreateSubKey("DefaultIcon"))
                    {
                        defaultIconKey.SetValue("", Path.Combine(Constants.IconDir, Constants.Icons[0]));
                    }

                    using (var shellKey = markpadKey.CreateSubKey("shell"))
                    {
                        using (var openKey = shellKey.CreateSubKey("open"))
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
