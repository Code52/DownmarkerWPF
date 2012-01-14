namespace MarkPad.Document
{
    public class MarkPadHyperlink
    {
        public MarkPadHyperlink(string text, string url)
        {
            Set(text, url);
        }

        public void Set(string text, string url)
        {
            Text = text;
            Url = url;
        }

        public string Text { get; private set; }
        public string Url { get; private set; }
    }
}