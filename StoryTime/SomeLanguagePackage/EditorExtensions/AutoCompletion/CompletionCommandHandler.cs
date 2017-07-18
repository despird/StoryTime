using System;
using System.Linq;
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
using System.IO;
using SOME.SomeLanguageCore;
using Microsoft.VisualStudio.Text.Projection;
using SOME.SomeLanguageCore.Parser;
using Microsoft.VisualStudio.Text.Operations;

namespace SOME.SomeLanguageService.EditorExtensions.AutoCompletion
{

    internal class SomeCompletionCommandHandler : IOleCommandTarget
    {
        private IOleCommandTarget m_nextCommandHandler;
        private ITextView m_textView;
        private CompletionCommandHandlerProvider m_provider;
        private ICompletionSession m_session;
        private ICompletionBroker m_broker;
        private IEditorOperations m_operationService;

        internal SomeCompletionCommandHandler(IVsTextView textViewAdapter, ICompletionBroker broker, ITextView textView, IEditorOperations operationService, CompletionCommandHandlerProvider provider)
        {
            this.m_textView = textView;
            this.m_provider = provider;
            this.m_broker = broker;
            this.m_operationService = operationService;

            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out m_nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return m_nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (VsShellUtilities.IsInAutomationFunction(m_provider.ServiceProvider))
            {
                return m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            //make a copy of this so we can look at it after forwarding some commands
            uint commandID = nCmdID;
            char typedChar = char.MinValue;
            //make sure the input is a char before getting it
            if (pguidCmdGroup == VSConstants.VSStd2K && nCmdID == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
            {
                typedChar = (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            }

            //check for a commit character
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN
                || nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB
                || ((char.IsWhiteSpace(typedChar) || char.IsPunctuation(typedChar)) && typedChar != SomeChar.ULine))
            {
                //check for a a selection
                if (m_session != null && !m_session.IsDismissed)
                {
                    //if the selection is fully selected, commit the current session
                    if (m_session.SelectedCompletionSet.SelectionStatus.IsSelected)
                    {
                        m_session.Commit();
                        if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN)
                        {
                            //don't send the charactor to butter
                            return VSConstants.S_OK;
                        }

                        //if . is typed, keep going as new session could be triggered.
                        if (typedChar != SomeChar.Dot && typedChar != SomeChar.Less)
                        {
                            return m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
                        }
                    }
                    else
                    {
                        //if there is no selection, dismiss the session
                        m_session.Dismiss();
                    }
                }
            }

            //pass along the command so the char is added to the buffer
            int retVal = m_nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            bool handled = false;
            SnapshotPoint? caretPoint;
            if (ShouldTrigger(typedChar, out caretPoint))
            {
                if (m_session == null || m_session.IsDismissed) // If there is no active session, bring up completion
                {
                    this.TriggerCompletion(caretPoint);
                    if (m_session != null)
                    {
                        m_session.Filter();
                    }
                }
                else    //the completion session is already active, so just filter
                {
                    m_session.Filter();
                }
                handled = true;
            }
            else if (commandID == (uint)VSConstants.VSStd2KCmdID.BACKSPACE   //redo the filter if there is a deletion
                || commandID == (uint)VSConstants.VSStd2KCmdID.DELETE)
            {
                if (m_session != null && !m_session.IsDismissed)
                    m_session.Filter();
                handled = true;
            }
            if (handled) return VSConstants.S_OK;
            if (typedChar == SomeChar.LCurly)
            {
                m_operationService.InsertText(SomeChar.RCurly.ToString());
                m_operationService.MoveToPreviousCharacter(false);
            }
            return retVal;
        }

        private bool ShouldTrigger(char c, out SnapshotPoint? caretPoint)
        {
            caretPoint = getCaretPoint();
            bool shouldTrigger = !c.Equals(char.MinValue) && char.IsLetterOrDigit(c) || c == SomeChar.Dot || c == SomeChar.Less;
            if (shouldTrigger == false)
                return false;

            ITextBuffer textBuffer = this.m_textView.TextBuffer;

            int position = caretPoint.Value.Position;
            int line = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);

            string content = textBuffer.CurrentSnapshot.GetText();
            //string currentLine = textBuffer.CurrentSnapshot.Lines.ElementAt(line).GetText();
            string contentUtil = content.Substring(0, position);

            string fileExt = Path.GetExtension(this.m_textView.TextBuffer.GetFileName());
            if (fileExt == SomeLanguageService.LANGUAGE_EXT1)
            {
                SomeCompiler.ParseTypes(contentUtil, "default");
                return SomeParser.ParsingStateType != EditingStateType.Nothing;
            }
            else //fileExt == SomeLanguageService.LANGUAGE_EXT2
            {
                SomeCompiler.ParseSequence(contentUtil);
                return SomeParser.ParsingStateType != EditingStateType.Nothing;
            }
        }

        private bool TriggerCompletion(SnapshotPoint? caretPoint)
        {
            m_session = m_broker.CreateCompletionSession(m_textView,
                caretPoint.Value.Snapshot.CreateTrackingPoint(caretPoint.Value.Position, PointTrackingMode.Positive),
                true);

            //m_session = m_broker.TriggerCompletion(m_textView);

            //subscribe to the Dismissed event on the session 
            m_session.Dismissed += this.OnSessionDismissed;
            m_session.Start();

            return true;
        }

        private SnapshotPoint? getCaretPoint()
        {
            SnapshotPoint? caretPoint;
            //the caret must be in a non-projection location 
            caretPoint =
            m_textView.Caret.Position.Point.GetPoint(
            textBuffer => (!textBuffer.ContentType.IsOfType("projection")), PositionAffinity.Predecessor);

            if (!caretPoint.HasValue)
            {
                return null;
            }

            return caretPoint;
        }

        private void OnSessionDismissed(object sender, EventArgs e)
        {
            m_session.Dismissed -= this.OnSessionDismissed;
            m_session = null;
        }
    }
}
