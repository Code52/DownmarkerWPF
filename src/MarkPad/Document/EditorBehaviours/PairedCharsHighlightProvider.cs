using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;

namespace MarkPad.Document.EditorBehaviours
{
    class PairedCharsHighlightProvider : IPairedCharsHighlightProvider
    {
        private readonly PairedCharacterRenderer pairedCharacterRenderer;
        private DocumentView view;

        public PairedCharsHighlightProvider()
        {
            pairedCharacterRenderer = new PairedCharacterRenderer();
        }

        public void Initialise(DocumentView documentView)
        {
            view = documentView;
            view.TextView.BackgroundRenderers.Add(pairedCharacterRenderer);
            view.TextView.VisualLinesChanged += TextViewVisualLinesChanged;
            view.Editor.TextArea.Caret.PositionChanged += TextAreaCaretPositionChanged;
        }

        public void Disconnect()
        {
            if (view == null) return;
            view.TextView.BackgroundRenderers.Remove(pairedCharacterRenderer);
            view.TextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            view = null;
        }

        void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            MarkPreviousChar();
        }

        void TextAreaCaretPositionChanged(object sender, EventArgs e)
        {
            MarkPreviousChar(); 
            
            //force a refresh
            view.Editor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
        }

        void MarkPreviousChar()
        {
            //clear any existing highlights
            pairedCharacterRenderer.PairedCharacters.Clear();

            var caretOffset = view.Editor.CaretOffset;

            //check if the current character has a paired character
            var currentChar = view.Editor.GetPrevCharacter();
            if (AutoPairedCharacters.IsValidOpeningCharacter(currentChar))
            {
                var closePos = AutoPairedCharacters.GetClosingCharPosition(currentChar, view.Editor.CaretOffset, view.Editor);
                if (closePos != -1)
                {
                    pairedCharacterRenderer.PairedCharacters.Add(new TextSegment
                    {
                        StartOffset = caretOffset - 1,
                        Length = 1
                    });

                    pairedCharacterRenderer.PairedCharacters.Add(new TextSegment
                    {
                        StartOffset = closePos,
                        Length = 1
                    });
                }
            }
            else if (AutoPairedCharacters.IsValidClosingCharacter(currentChar))
            {
                var openPos = AutoPairedCharacters.GetOpeningCharPosition(currentChar, view.Editor.CaretOffset - 1, view.Editor);
                if (openPos != -1)
                {
                    pairedCharacterRenderer.PairedCharacters.Add(new TextSegment
                    {
                        StartOffset = caretOffset - 1,
                        Length = 1
                    });

                    pairedCharacterRenderer.PairedCharacters.Add(new TextSegment
                    {
                        StartOffset = openPos,
                        Length = 1
                    });
                }
            }
        }
    }
}
