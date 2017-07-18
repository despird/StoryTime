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
    [Export(typeof(EditorFormatDefinition))]
    [Name("SomeHighlightWordFormatDefinition")]        //TODO: using GUID for the name to avoid conflict
    [UserVisible(true)]
    internal class HighlightWordFormatDefinition : MarkerFormatDefinition
    {
        public HighlightWordFormatDefinition()
        {
            this.BackgroundColor = Colors.LightGray;
            this.ForegroundColor = Colors.LightGray;
            this.DisplayName = "Highlight Word";
            this.ZOrder = 5;
        }
    }
}
