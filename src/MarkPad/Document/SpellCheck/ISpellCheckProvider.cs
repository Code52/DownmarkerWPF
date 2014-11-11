using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;

namespace MarkPad.Document.SpellCheck
{
	public interface ISpellCheckProvider
	{
	    void Initialise(DocumentView documentView);
	    IEnumerable<TextSegment> GetSpellCheckErrors();
	    IEnumerable<string> GetSpellcheckSuggestions(string word);
        void AddWordToCustomDictionary(string word);
	    void Disconnect();
	}
}
