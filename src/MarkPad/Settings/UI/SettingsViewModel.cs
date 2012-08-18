using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media;
using Caliburn.Micro;
using MarkPad.Document.SpellCheck;
using MarkPad.Events;
using MarkPad.Framework;
using MarkPad.Plugins;
using MarkPad.PreviewControl;
using MarkPad.Settings.Models;
using Microsoft.Win32;

namespace MarkPad.Settings.UI
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
        public IndentType IndentType { get; set; }

        readonly IList<IPlugin> plugins;

        public SettingsViewModel(
            ISettingsProvider settingsService,
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            Func<BlogSettingsViewModel> blogSettingsCreator,
            Func<IPlugin, PluginViewModel> pluginViewModelCreator,
            ISpellingService spellingService, 
            IEnumerable<IPlugin> plugins)
        {
            this.settingsService = settingsService;
            this.windowManager = windowManager;
            this.eventAggregator = eventAggregator;
            this.blogSettingsCreator = blogSettingsCreator;
            this.pluginViewModelCreator = pluginViewModelCreator;
            this.spellingService = spellingService;
            this.plugins = plugins.ToList();
        }

        public void Initialize()
        {
            InitialiseExtensions();

            var settings = settingsService.GetSettings<MarkPadSettings>();
            var blogs = settings.GetBlogs();

            Blogs = new ObservableCollection<BlogSetting>(blogs);

            Languages = Enum.GetValues(typeof(SpellingLanguages)).OfType<SpellingLanguages>().ToArray();
            FontSizes = Enum.GetValues(typeof(FontSizes)).OfType<FontSizes>().ToArray();
            FontFamilies = Fonts.SystemFontFamilies.OrderBy(f => f.Source);

            SelectedLanguage = settings.Language;

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

        private void InitialiseExtensions()
        {
            var softwareKey = Registry.CurrentUser.OpenSubKey("Software");
            if (softwareKey == null) return;
            using (var key = softwareKey.OpenSubKey("Classes"))
            {
                if (key == null)
                {
                    Extensions = new ExtensionViewModel[0];
                    return;
                }

                Extensions = Constants.DefaultExtensions
                    .Select(s =>
                    {
                        
                        var openSubKey = key.OpenSubKey(s);
                        return new ExtensionViewModel(s, Enabled(s, key, openSubKey));
                    })
                    .Where(e => e != null)
                    .ToArray();
            }
        }

        private static bool Enabled(string s, RegistryKey key, RegistryKey openSubKey)
        {
            return openSubKey != null &&
                   (key.GetSubKeyNames().Contains(s) &&
                    !string.IsNullOrEmpty(openSubKey.GetValue("").ToString()));
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

        public ObservableCollection<IndentType> IndentTypes
        {
            get { return new ObservableCollection<IndentType> { IndentType.Tabs, IndentType.Spaces }; }
        }

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
            settings.IndentType = IndentType;
            settings.Language = SelectedLanguage;

            settingsService.SaveSettings(settings);

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

            var software = Registry.CurrentUser.OpenSubKey("Software");
            if (software == null) return;
            using (var classesKey = software.OpenSubKey("Classes", true))
            {
                if (classesKey == null) return;
                foreach (var ext in Extensions)
                {
                    using (var extensionKey = classesKey.CreateSubKey(ext.Extension))
                    {
                        if (extensionKey != null)
                            extensionKey.SetValue("", ext.Enabled ? MarkpadKeyName : "");
                    }
                }

                using (var markpadKey = classesKey.CreateSubKey(MarkpadKeyName))
                {
                    if (markpadKey == null) return;
                    using (var defaultIconKey = markpadKey.CreateSubKey("DefaultIcon"))
                    {
                        if (defaultIconKey != null)
                            defaultIconKey.SetValue("", Path.Combine(Constants.IconDir, Constants.Icons[0]));
                    }

                    using (var shellKey = markpadKey.CreateSubKey("shell"))
                    {
                        if (shellKey == null) return;
                        using (var openKey = shellKey.CreateSubKey("open"))
                        {
                            if (openKey == null) return;
                            using (var commandKey = openKey.CreateSubKey("command"))
                            {
                                if (commandKey != null) commandKey.SetValue("", "\"" + exePath + "\" \"%1\"");
                            }
                        }
                    }
                }
            }
        }
    }
}
