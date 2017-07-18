using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace SOME.SomeLanguageService.EditorExtensions.Highlighting
{
    internal class HighlightWordTag : TextMarkerTag
    {
        //TODO: using GUID for the name to avoid conflict
        public HighlightWordTag() : base("SomeHighlightWordFormatDefinition") { }

    }
}
