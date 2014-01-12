using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MarkPad.Document.EditorBehaviours
{
    public interface IPairedCharsHighlightProvider
    {
        void Initialise(DocumentView documentView);
        void Disconnect();
    }
}
