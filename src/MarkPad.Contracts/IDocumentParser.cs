namespace MarkPad.Contracts
{
	public interface IDocumentParser
	{
		string Parse(string source);
		string ParseClean(string source);
	}
}
