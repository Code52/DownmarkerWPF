using System;
using System.Windows;
using Caliburn.Micro;

namespace MarkPad.HyperlinkEditor
{
    public class HyperlinkEditorViewModel : Screen
    {
        //private string initialText;
        //private string initialUrl;

        public HyperlinkEditorViewModel(string text, string url)
        {
            //initialText = 
            Text = text;
            //initialUrl = 
            Url = url;
            WasCancelled = false;
        }

        public bool WasCancelled { get; private set; }

        public string Text { get; set; }
        public string Url { get; set; }

        public void Cancel()
        {
            //Text = initialText;
            //Url = initialUrl;

            WasCancelled = true;
            TryClose();
        }

        public void Close()
        {
            TryClose();
        }
    }
}
