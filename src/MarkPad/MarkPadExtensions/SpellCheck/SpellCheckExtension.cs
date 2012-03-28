using System;
using System.Collections.Generic;
using System.Linq;
using MarkPad.Document;
using MarkPad.Services.Interfaces;

namespace MarkPad.MarkPadExtensions.SpellCheck
{
	public class SpellCheckExtension : IDocumentViewExtension
	{
		public string Name { get { return "Spell check"; } }

		readonly ISpellingService _spellingService;

		IList<SpellCheckProvider> _providers = new List<SpellCheckProvider>();

		public SpellCheckExtension(ISpellingService spellingService)
		{
			_spellingService = spellingService;
		}

		public void ConnectToDocumentView(DocumentView view)
		{
			if (_providers.Any(p => p.View == view)) throw new ArgumentException("View already has a spell check provider connected", "view");
			
			var provider = new SpellCheckProvider(_spellingService, view);
			_providers.Add(provider);
		}

		public void DisconnectFromDocumentView(DocumentView view)
		{
			var provider = _providers.FirstOrDefault(p => p.View == view);
			if (provider == null) return;

			provider.Disconnect();
			_providers.Remove(provider);
		}
	}
}
