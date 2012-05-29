using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

namespace MarkPad.Contracts
{
	public interface ISpellCheckProviderFactory
	{
		ISpellCheckProvider GetProvider(ISpellingService spellingService, IDocumentView view);
	}
}
