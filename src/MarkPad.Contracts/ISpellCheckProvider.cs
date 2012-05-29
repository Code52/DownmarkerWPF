namespace MarkPad.Contracts
{
	public interface ISpellCheckProvider
	{
		IDocumentView View { get; }
		void Disconnect();
	}
}
