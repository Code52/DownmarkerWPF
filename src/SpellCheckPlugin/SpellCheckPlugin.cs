using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using MarkPad.Contracts;
using MarkPad.PluginApi;
using System.ComponentModel;

namespace SpellCheckPlugin
{
	public class SpellCheckPlugin : IDocumentViewPlugin
	{
		IPluginSettingsProvider _settingsProvider;
		ISpellingService _spellingService;
		ISpellCheckProviderFactory _spellCheckProviderFactory;
        
		public string Name { get { return "Spell check"; } }
		public string Version {get{return "0.9";}}
		public string Authors { get { return "Code52"; } }
		public string Description { get { return "Built-in spell check support"; } }
		SpellCheckPluginSettings _settings;
		public IPluginSettings Settings { get { return _settings; } }
		IList<ISpellCheckProvider> _providers = new List<ISpellCheckProvider>();

		[ImportingConstructor]
		public SpellCheckPlugin(
			IPluginSettingsProvider settingsProvider,
			ISpellingService spellingService,
			ISpellCheckProviderFactory spellCheckProviderFactory)
		{
			_settingsProvider = settingsProvider;
			_spellingService = spellingService;
			_spellCheckProviderFactory = spellCheckProviderFactory;

			_settings = _settingsProvider.GetSettings<SpellCheckPluginSettings>();
			_spellingService.SetLanguage(_settings.Language);
		}

		public void SaveSettings() { _settingsProvider.SaveSettings(_settings); }

        public void ConnectToDocumentView(IDocumentView view)
        {
			if (_providers.Any(p => p.View == view))
			{
				throw new ArgumentException("View already has a spell check provider connected", "view");
			}

			var provider = _spellCheckProviderFactory.GetProvider(_spellingService, view);
            _providers.Add(provider);
        }

        public void DisconnectFromDocumentView(IDocumentView view)
        {
            var provider = _providers.FirstOrDefault(p => p.View == view);
            if (provider == null) return;

            provider.Disconnect();
            _providers.Remove(provider);
        }
    }

	public class SpellCheckPluginSettings : PluginSettings
	{
        [DefaultValue(SpellingLanguages.Australian)]
        public SpellingLanguages Language { get; set; }
	}
}
