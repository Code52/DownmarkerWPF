using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.PluginApi;
using MarkPad.Contracts;
using System.ComponentModel.Composition;
using Analects.DialogService;
using System.IO;
using System.ComponentModel;

namespace ExportToHtmlPlugin
{
	public class ExportToHtmlPlugin : ICanSavePage
	{
		const string PLUGIN_NAME = "Export to HTML";
		const string PLUGIN_VERSION = "0.1";
		const string PLUGIN_AUTHORS = "Code52";
		const string PLUGIN_DESCRIPTION = "Export the file as HTML (using the rendered view)";
		const string SAVE_PAGE_LABEL = "Save HTML";
		const string SAVE_TITLE = "Choose a location to save the HTML document";
		const string SAVE_DEFAULT_EXT = "*.html";
		const string SAVE_FILTER = "HTML Files (*.html,*.htm)|*.html;*.htm|All Files (*.*)|*.*";

		readonly IDocumentParser _documentParser;
		readonly IPluginSettingsProvider _settingsProvider;

		public string Name { get { return PLUGIN_NAME; } }
		public string Version { get { return PLUGIN_VERSION; } }
		public string Authors { get { return PLUGIN_AUTHORS; } }
		public string Description { get { return PLUGIN_DESCRIPTION; } }
		public string SavePageLabel { get { return SAVE_PAGE_LABEL; } }
		ExportToHtmlPluginSettings _settings;
		public IPluginSettings Settings { get { return _settings; } }
		public void SaveSettings() { _settingsProvider.SaveSettings(_settings); }
		public bool IsConfigurable { get { return false; } }
		public bool IsHidden { get { return false; } }

		[ImportingConstructor]
		public ExportToHtmlPlugin(
			IDocumentParser documentParser,
			IPluginSettingsProvider settingsProvider)
		{
			_documentParser = documentParser;
			_settingsProvider = settingsProvider;

			_settings = _settingsProvider.GetSettings<ExportToHtmlPluginSettings>();
		}

		public void SavePage(IDocumentViewModel documentViewModel)
		{
			if (documentViewModel == null) return;

			var dialogService = new DialogService();
			var filename = dialogService.GetFileSavePath(SAVE_TITLE, SAVE_DEFAULT_EXT, SAVE_FILTER);
			if (string.IsNullOrEmpty(filename)) return;

			var markdown = documentViewModel.MarkdownContent;
			var html = _documentParser.ParseClean(markdown);

			File.WriteAllText(filename, html);
		}

	}

	public class ExportToHtmlPluginSettings : IPluginSettings
	{
		[DefaultValue(false)]
		public bool IsEnabled { get; set; }
	}
}
