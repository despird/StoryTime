using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using SOME.SomeLanguageCore.Tokens;
using Microsoft.VisualStudio.Text.Operations;

namespace SOME.SomeLanguageService.EditorExtensions.AutoCompletion
{

    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Some token completion handler")]
    [ContentType(SomeContentTypeDefinition.ContentType1)]
    [ContentType(SomeContentTypeDefinition.ContentType2)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class CompletionCommandHandlerProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;
        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }
        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }
        [Import]
        internal IEditorOperationsFactoryService OperationsService = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;

            IEditorOperations operations = OperationsService.GetEditorOperations(textView);


            Func<SomeCompletionCommandHandler> createCommandHandler = delegate() 
            {
                return new SomeCompletionCommandHandler(textViewAdapter, CompletionBroker, textView, operations, this); 
            };
            textView.Properties.GetOrCreateSingletonProperty(createCommandHandler);
        }
    }

}
