using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using MarkPad.Document.SpellCheck;
using MarkPad.DocumentSources.MetaWeblog;
using MarkPad.Events;
using MarkPad.Framework;
using MarkPad.Plugins;
using MarkPad.PreviewControl;
using MarkPad.Settings.Models;

namespace MarkPad.Settings.UI
{
    public class SettingsViewModel : Screen
    {
        public const string FontSizeSettingsKey = "Font";
        public const string FontFamilySettingsKey = "FontFamily";


        private readonly ISettingsProvider settingsProvider;
        private readonly IEventAggregator eventAggregator;
        private readonly Func<IPlugin, PluginViewModel> pluginViewModelCreator;
        private readonly ISpellingService spellingService;
        private readonly IList<IPlugin> plugins;
        private readonly IBlogService blogService;
        private readonly IMarkpadRegistryEditor markpadRegistryEditor;
        private BlogSetting currentBlog;

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
        public bool EnableMarkdownExtra { get; set; }
        public bool CanEditBlog { get { return currentBlog != null; } }
        public bool CanRemoveBlog { get { return currentBlog != null; } }
        public bool IsColorsInverted { get; set; }
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

        public ObservableCollection<IndentType> IndentTypes
        {
            get { return new ObservableCollection<IndentType> { IndentType.Tabs, IndentType.Spaces }; }
        }
        

        public SettingsViewModel(
            ISettingsProvider settingsProvider,
            IEventAggregator eventAggregator,
            Func<IPlugin, PluginViewModel> pluginViewModelCreator,
            ISpellingService spellingService, 
            IEnumerable<IPlugin> plugins, 
            IBlogService blogService,
            IMarkpadRegistryEditor markpadRegistryEditor)
        {
            this.settingsProvider = settingsProvider;
            this.eventAggregator = eventAggregator;
            this.pluginViewModelCreator = pluginViewModelCreator;
            this.spellingService = spellingService;
            this.blogService = blogService;
            this.plugins = plugins.ToList();
            this.markpadRegistryEditor = markpadRegistryEditor;
        }


        public void Initialize()
        {
            Extensions = markpadRegistryEditor.GetExtensionsFromRegistry();

            var settings = settingsProvider.GetSettings<MarkPadSettings>();
            var blogs = blogService.GetBlogs();

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
            IsColorsInverted = settings.IsEditorColorsInverted;

            Plugins = plugins
                .Where(plugin => !plugin.IsHidden)
                .Select(plugin => pluginViewModelCreator(plugin));

            EnableMarkdownExtra = settings.MarkdownExtraEnabled;
        }
        

        public bool AddBlog()
        {
            var blog = blogService.AddBlog();

            if (blog != null)
            {
                Blogs.Add(blog);
                return true;
            }

            return false;
        }


        public void EditBlog()
        {
            if (CurrentBlog == null) return;

            blogService.EditBlog(CurrentBlog);
        }


        public void RemoveBlog()
        {
            if (CurrentBlog != null)
            {
                blogService.Remove(CurrentBlog);
                Blogs.Remove(CurrentBlog);
            }
        }

        public void ResetFont()
        {
            SelectedFontFamily = FontHelpers.TryGetFontFamilyFromStack(Constants.DEFAULT_EDITOR_FONT_FAMILY);
            SelectedFontSize = Constants.DEFAULT_EDITOR_FONT_SIZE;
            IsColorsInverted = false;
        }

        public void Accept()
        {
            markpadRegistryEditor.UpdateExtensionRegistryKeys(Extensions);

            spellingService.SetLanguage(SelectedLanguage);

            UpdateMarkpadSettings();

            eventAggregator.Publish(new SettingsChangedEvent());
        }


        public void HideSettings()
        {
            eventAggregator.Publish(new SettingsCloseEvent());
            Accept();
        }

        private void UpdateMarkpadSettings()
        {
            var settings = settingsProvider.GetSettings<MarkPadSettings>();

            settings.FontSize = SelectedFontSize;
            settings.FontFamily = SelectedFontFamily.Source;
            settings.FloatingToolBarEnabled = EnableFloatingToolBar;
            settings.IsEditorColorsInverted = IsColorsInverted;
            settings.IndentType = IndentType;
            settings.Language = SelectedLanguage;
            settings.MarkdownExtraEnabled = EnableMarkdownExtra;

            settingsProvider.SaveSettings(settings);
        }
    }
}
