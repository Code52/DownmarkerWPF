﻿using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using MarkPad.Framework;
using MarkPad.Framework.Events;
using MarkPad.XAML;

namespace MarkPad.Document
{
    public partial class DocumentView
    {
        private ScrollViewer documentScrollViewer;
        private readonly IEventAggregator eventAggregator;
        public DocumentView()
        {
            eventAggregator = IoC.Get<IEventAggregator>();

            InitializeComponent();
            Loaded += DocumentViewLoaded;
            wb.Loaded += WbLoaded;

            SizeChanged += new SizeChangedEventHandler(DocumentViewSizeChanged);

            Editor.TextArea.SelectionChanged += SelectionChanged;

            Editor.PreviewMouseLeftButtonUp += HandleMouseUp;

            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleBold, (x, y) => ToggleBold(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleItalic, (x, y) => ToggleItalic(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleCode, (x, y) => ToggleCode(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleCodeBlock, (x, y) => ToggleCodeBlock(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.SetHyperlink, (x, y) => SetHyperlink(), CanEditDocument));
        }

        void DocumentViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Hide web browser when the window is too small for it to make much sense
            if (e.NewSize.Width <= 350)
            {
                webBrowserColumn.MaxWidth = 0;
            }
            else
            {
                webBrowserColumn.MaxWidth = double.MaxValue;
            }
        }

        void WbLoaded(object sender, RoutedEventArgs e)
        {
            wb.ExecuteJavascript("window.scrollTo(0," + documentScrollViewer.VerticalOffset + ");");
        }

        private void DocumentViewLoaded(object sender, RoutedEventArgs e)
        {
            using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("MarkPad.Syntax.Markdown.xshd"))
            using (var reader = new XmlTextReader(stream))
            {
                Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            documentScrollViewer = Editor.FindVisualChild<ScrollViewer>();

            if (documentScrollViewer != null)
            {
                documentScrollViewer.ScrollChanged += (i, j) => wb.ExecuteJavascript("window.scrollTo(0," + j.VerticalOffset + ");");
                var x = ((DocumentViewModel)DataContext);
                x.Document.TextChanged += (i, j) =>
                                              {
                                                  wb.LoadCompleted += (k, l) => wb.ExecuteJavascript("window.scrollTo(0," + documentScrollViewer.VerticalOffset + ");");
                                              };
            }

            //  AvalonEdit hijacks Ctrl+I. We need to free that mutha up
            var editCommandBindings = Editor.TextArea.DefaultInputHandler.Editing.CommandBindings;

            editCommandBindings
                .FirstOrDefault(b => b.Command == ICSharpCode.AvalonEdit.AvalonEditCommands.IndentSelection)
                .ExecuteSafely(b => editCommandBindings.Remove(b));

            // set default focus to the editor
            Editor.Focus();
        }


        internal void ToggleBold()
        {
            var selectedText = GetSelectedText();
            if (string.IsNullOrWhiteSpace(selectedText)) return;

            Editor.SelectedText = selectedText.ToggleBold(!selectedText.IsBold());
        }

        internal void ToggleItalic()
        {
            var selectedText = GetSelectedText();
            if (string.IsNullOrWhiteSpace(selectedText)) return;

            Editor.SelectedText = selectedText.ToggleItalic(!selectedText.IsItalic());
        }

        internal void ToggleCode()
        {
            if (Editor.SelectedText.Contains(NewLine))
                ToggleCodeBlock();
            else
            {
                var selectedText = GetSelectedText();
                if (string.IsNullOrWhiteSpace(selectedText)) return;

                Editor.SelectedText = selectedText.ToggleCode(!selectedText.IsCode());
            }
        }


        private string GetSelectedText()
        {
            var textArea = Editor.TextArea;
            // What would you do if the selected text is empty? I vote: Nothing.
            if (textArea.Selection.IsEmpty)
                return null;

            return textArea.Selection.GetText(textArea.Document);
        }

        private const string NewLine = "\r\n";
        private const int NumSpaces = 4;
        private const string Spaces = "    ";
        private void ToggleCodeBlock()
        {
            var lines = Editor.SelectedText.Split(NewLine.ToCharArray());
            if (lines[0].Length > 4)
            {
                if (lines[0].Substring(0, 4) == Spaces)
                {
                    Editor.SelectedText = Editor.SelectedText.Replace((NewLine + Spaces), NewLine);

                    // remember the first line
                    if (Editor.SelectedText.Length >= NumSpaces)
                    {
                        var firstFour = Editor.SelectedText.Substring(0, NumSpaces);
                        var rest = Editor.SelectedText.Substring(NumSpaces);

                        Editor.SelectedText = firstFour.Replace(Spaces, string.Empty) + rest;
                    }
                    return;
                }
            }

            Editor.SelectedText = Spaces + Editor.SelectedText.Replace(NewLine, NewLine + Spaces);
        }

        static readonly char[] WordBreakers = new[] { ' ', '\r', '\n' };

        internal void SetHyperlink()
        {
            var textArea = Editor.TextArea;
            if (textArea.Selection.IsEmpty)
                return;

            var selectedText = textArea.Selection.GetText(textArea.Document);

            //  Check if the selected text already is a link...
            string text = selectedText, url = string.Empty;
            var match = Regex.Match(selectedText, @"\[(?<text>(?:[^\\]|\\.)+)\]\((?<url>[^)]+)\)");
            if (match.Success)
            {
                text = match.Groups["text"].Value;
                url = match.Groups["url"].Value;
            }
            var hyperlink = new MarkPadHyperlink(text, url);

            (DataContext as DocumentViewModel)
                .ExecuteSafely(vm =>
                                   {
                                       hyperlink = vm.GetHyperlink(hyperlink);
                                       textArea.Selection.ReplaceSelectionWithText(textArea, 
                                           string.Format("[{0}]({1})", hyperlink.Text, hyperlink.Url));
                                   });
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (Editor.TextArea.Selection.IsEmpty)
            {
                floatingToolBar.Hide();
            }
        }

        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Editor.TextArea.Selection.IsEmpty)
            {
                floatingToolBar.Hide();
            }
            else
            {
                floatingToolBar.Show();
            }
        }

        private void CanEditDocument(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Editor != null && Editor.TextArea != null && Editor.TextArea.Selection != null)
            {
                e.CanExecute = !Editor.TextArea.Selection.IsEmpty;
            }
        }
    }
}
