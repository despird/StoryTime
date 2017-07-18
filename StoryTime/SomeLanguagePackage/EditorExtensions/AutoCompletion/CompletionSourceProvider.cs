using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Operations;

namespace SOME.SomeLanguageService.EditorExtensions.AutoCompletion
{
    [Export(typeof(ICompletionSourceProvider))]
	[ContentType(SomeContentTypeDefinition.ContentType1)]
    [ContentType(SomeContentTypeDefinition.ContentType2)]
    [Name("Some Completion Source Provider")]
	[Order(Before = "default")]
	internal class CompletionSourceProvider : ICompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new SomeCompletionSource(this, textBuffer);
        }
    }
}