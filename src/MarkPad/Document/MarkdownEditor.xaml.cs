﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Caliburn.Micro;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Rendering;
using MarkPad.Document.AvalonEditPreviewKeyDownHandlers;
using MarkPad.Document.EditorBehaviours;
using MarkPad.Framework;
using MarkPad.Framework.Events;

namespace MarkPad.Document
{
    public partial class MarkdownEditor
    {
        const int NumSpaces = 4;
        const string Spaces = "    ";

        IEnumerable<IHandle<EditorPreviewKeyDownEvent>> editorPreviewKeyDownHandlers;
        IEnumerable<IHandle<EditorTextEnteringEvent>> editorTextEnteringHandlers;

        public MarkdownEditor()
        {
            InitializeComponent();

            Editor.TextArea.SelectionChanged += SelectionChanged;
            Editor.PreviewMouseLeftButtonUp += HandleMouseUp;
            Editor.MouseMove += HandleEditorMouseMove;
            Editor.PreviewMouseLeftButtonDown += HandleEditorPreviewMouseLeftButtonDown;

            Editor.MouseMove += (s, e) => e.Handled = true;
            Editor.TextArea.TextEntering += new TextCompositionEventHandler(TextArea_TextEntering);

            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleBold, (x, y) => ToggleBold(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleItalic, (x, y) => ToggleItalic(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleCode, (x, y) => ToggleCode(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.ToggleCodeBlock, (x, y) => ToggleCodeBlock(), CanEditDocument));
            CommandBindings.Add(new CommandBinding(FormattingCommands.SetHyperlink, (x, y) => SetHyperlink(),
                                                   CanEditDocument));

            var overtypeMode = new OvertypeMode();

            editorPreviewKeyDownHandlers = new IHandle<EditorPreviewKeyDownEvent>[] {
                new CopyLeadingWhitespaceOnNewLine(),
                new PasteImagesUsingSiteContext(),
                new CursorLeftRightWithSelection(),
                new ControlRightTweakedForMarkdown(),
                new HardLineBreak(),
                overtypeMode
            };
            editorTextEnteringHandlers = new IHandle<EditorTextEnteringEvent>[] {
                overtypeMode
            };
        }

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(TextDocument), typeof(MarkdownEditor), new PropertyMetadata(default(TextDocument)));

        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        #region public bool FloatingToolbarEnabled
        public static readonly DependencyProperty FloatingToolbarEnabledProperty =
            DependencyProperty.Register("FloatingToolbarEnabled", typeof (bool), typeof (MarkdownEditor), new PropertyMetadata(default(bool)));

        public bool FloatingToolbarEnabled
        {
            get { return (bool)GetValue(FloatingToolbarEnabledProperty); }
            set { SetValue(FloatingToolbarEnabledProperty, value); }
        }
        #endregion

        #region public double EditorFontSize
        public static DependencyProperty EditorFontSizeProperty = DependencyProperty.Register("EditorFontSize", typeof (double), typeof (MarkdownEditor), 
            new PropertyMetadata(default(double), EditorFontSizeChanged));

        public double EditorFontSize
        {
            get { return (double) GetValue(EditorFontSizeProperty); }
            set { SetValue(EditorFontSizeProperty, value); }
        }
        #endregion

        private static void EditorFontSizeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((MarkdownEditor) dependencyObject).Editor.FontSize = (double)dependencyPropertyChangedEventArgs.NewValue;
        }

        private void EditorLoaded(object sender, RoutedEventArgs e)
        {
            using (var stream = Assembly.GetEntryAssembly().GetManifestResourceStream("MarkPad.Syntax.Markdown.xshd"))
            using (var reader = new XmlTextReader(stream))
            {
                Editor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            }

            // AvalonEdit hijacks Ctrl+I. We need to free that mutha up
            var editCommandBindings = Editor.TextArea.DefaultInputHandler.Editing.CommandBindings;

            editCommandBindings
                .FirstOrDefault(b => b.Command == AvalonEditCommands.IndentSelection)
                .ExecuteSafely(b => editCommandBindings.Remove(b));

            Editor.Focus();
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            if (Editor.TextArea.Selection.IsEmpty)
                floatingToolBar.Hide();
            else
                ShowFloatingToolBar();
        }

        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!FloatingToolbarEnabled)
                return;

            if (Editor.TextArea.Selection.IsEmpty)
                floatingToolBar.Hide();
            else
                ShowFloatingToolBar();
        }

        void HandleEditorMouseMove(object sender, MouseEventArgs e)
        {
            // Bail out if tool bar is disabled, if there is no selection, or if the toolbar is already open
            if (!FloatingToolbarEnabled) return;
            if (string.IsNullOrEmpty(Editor.SelectedText)) return;
            if (floatingToolBar.IsOpen) return;
            if (e.LeftButton == MouseButtonState.Pressed) return;

            // Bail out if the mouse isn't over the markdownEditor
            var editorPosition = Editor.GetPositionFromPoint(e.GetPosition(Editor));
            if (!editorPosition.HasValue) return;

            // Bail out if the mouse isn't over a selection
            var offset = Editor.Document.GetOffset(editorPosition.Value.Line, editorPosition.Value.Column);
            if (offset < Editor.SelectionStart) return;
            if (offset > Editor.SelectionStart + Editor.SelectionLength) return;

            ShowFloatingToolBar();
        }

        void HandleEditorPreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (!floatingToolBar.IsOpen) return;
            floatingToolBar.Hide();
        }

        private void ShowFloatingToolBar()
        {
            // Find the screen position of the start of the selection
            var selectionStartLocation = Editor.Document.GetLocation(Editor.SelectionStart);
            var selectionStartPosition = new TextViewPosition(selectionStartLocation);
            var selectionStartPoint = Editor.TextArea.TextView.GetVisualPosition(selectionStartPosition, VisualYPosition.LineTop);

            var popupPoint = new Point(
                selectionStartPoint.X + 30,
                selectionStartPoint.Y - 35);

            floatingToolBar.Show(Editor, popupPoint);
        }

        private void EditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            foreach (var handler in editorPreviewKeyDownHandlers)
            {
                handler.Handle(new EditorPreviewKeyDownEvent(DataContext as DocumentViewModel, Editor, e));
            }
        }

        void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            foreach (var handler in editorTextEnteringHandlers)
            {
                handler.Handle(new EditorTextEnteringEvent(DataContext as DocumentViewModel, Editor, e));
            }
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
            if (Editor.SelectedText.Contains(Environment.NewLine))
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

            return textArea.Selection.GetText();
        }

        private void ToggleCodeBlock()
        {
            var lines = Editor.SelectedText.Split(Environment.NewLine.ToCharArray());
            if (lines[0].Length > 4)
            {
                if (lines[0].Substring(0, 4) == Spaces)
                {
                    Editor.SelectedText = Editor.SelectedText.Replace((Environment.NewLine + Spaces), Environment.NewLine);

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

            Editor.SelectedText = Spaces + Editor.SelectedText.Replace(Environment.NewLine, Environment.NewLine + Spaces);
        }

        internal void SetHyperlink()
        {
            var textArea = Editor.TextArea;
            if (textArea.Selection.IsEmpty)
                return;

            var selectedText = textArea.Selection.GetText();

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
                    if (hyperlink != null)
                    {
                        textArea.Selection.ReplaceSelectionWithText(string.Format("[{0}]({1})", hyperlink.Text, hyperlink.Url));
                    }
                });
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
