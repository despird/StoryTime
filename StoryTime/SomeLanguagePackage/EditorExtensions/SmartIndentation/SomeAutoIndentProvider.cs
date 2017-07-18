using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SOME.SomeLanguageService.EditorExtensions.SmartIndentation
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(SomeContentTypeDefinition.ContentType1)]
    [ContentType(SomeContentTypeDefinition.ContentType2)]
    public sealed class SomeAutoIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new SomeIndent(textView);
        }
    }
}
