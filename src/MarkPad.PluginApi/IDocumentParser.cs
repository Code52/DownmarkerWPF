namespace MarkPad.Plugins
{
	public interface IDocumentParser
	{
		string Parse(string source);
		string ParseClean(string source);
	}
}
