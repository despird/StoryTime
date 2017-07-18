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
    public sealed class SomeIndent : ISmartIndent
    {
        private readonly ITextView _textView;

        public SomeIndent(ITextView view)
        {
            _textView = view;
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            if (line.LineNumber == 0) return 0;

            //get indent count for previous line and do same for current
            var snapshot = line.Snapshot;
            var prevline = snapshot.GetLineFromLineNumber(line.LineNumber - 1);
            var curline = snapshot.GetLineFromLineNumber(line.LineNumber);
            string textPrev = prevline.GetText();
            string textCur = curline.GetText();

            int indentSize = 4; //TODOZ: using customised value

            for (int i = 0; i < textPrev.Length; i++)
            {
                if (!char.IsWhiteSpace(textPrev[i]))
                {
                    if (textPrev.TrimEnd().EndsWith("{"))
                    {
                        if (textCur.Trim().StartsWith("}"))
                        {
                            return i;
                        }
                        else
                        {
                            return i + indentSize;
                        }
                    }
                    else
                    {
                        return i;
                    }
                }
            }
            return null;
        }

        public void Dispose()
        {
        }
    }

}
