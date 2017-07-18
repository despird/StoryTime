using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using SOME.SomeLanguageCore.Tokens;
using SOME.SomeLanguageCore;
using SOME.SomeLanguageCore.CodeDom;
using System.IO;

namespace SOME.SomeLanguageService.EditorExtensions.Classification
{
    /// <summary>
    /// Implements <see cref="IClassifier"/> in order to provide coloring
    /// </summary>
    internal class SomeClassifier : IClassifier
    {
        public struct MultiLineToken
        {
            //Classification used teh token
            public IClassificationType Classification;
            //Tracked span of token
            public ITrackingSpan Tracking;
            //Version of text when Tracking was created
            public ITextVersion Version;
        }


        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
        private IClassificationTypeRegistryService classificationRegistryService;
        private ITextBuffer textBuffer;
        protected List<MultiLineToken> _multiLineTokens;

        internal SomeClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService classificationRegistryService)
        {
            this.textBuffer = textBuffer;
            
            //SomeTokenList tokenList = new SomeTokenList();
            //SomeCompiler.lex(tokenList, textBuffer.CurrentSnapshot.GetText());
            //SomeCompiler.TokenList = tokenList;
            //SomeCompiler.CodeName = "default";
            //SomeCompiler.ParseTypes();

            this.classificationRegistryService = classificationRegistryService;
            _multiLineTokens = new List<MultiLineToken>();
            this.textBuffer.ReadOnlyRegionsChanged += new EventHandler<SnapshotSpanEventArgs>(textBuffer_ReadOnlyRegionsChanged);
        }

        void textBuffer_ReadOnlyRegionsChanged(object sender, SnapshotSpanEventArgs e)
        {
            // We need to call this event when read-only regions are added, so they will be grayed out.
            Invalidate(new SnapshotSpan(textBuffer.CurrentSnapshot, e.Span));
        }

        //Invoke ClassificationChanged that cause editor to re-classify specified span 
        protected void Invalidate(SnapshotSpan span)
        {
            if (ClassificationChanged != null)
                ClassificationChanged(this, new ClassificationChangedEventArgs(span));
        }

        IList<ClassificationSpan> IClassifier.GetClassificationSpans(SnapshotSpan span)
        {
            var result = new List<ClassificationSpan>();

            bool isInsideMultiLine = false;

            //Scan for all know multi-line tokens, checking for current span intersection 
            for (var i = _multiLineTokens.Count - 1; i >= 0; i--)
            {
                var multiSpan = _multiLineTokens[i].Tracking.GetSpan(span.Snapshot);
                //Check if the span of the multi-line token is collapsed (zero length), and if true
                //remove it from the list
                if (multiSpan.Length == 0)
                    _multiLineTokens.RemoveAt(i);
                else
                {
                    if (span.IntersectsWith(multiSpan))
                    {
                        isInsideMultiLine = true;
                        //check if multi-line token is changed by comparing version of current 
                        //span with version on which token is found 
                        if (span.Snapshot.Version != _multiLineTokens[i].Version)
                        {
                            //if text inside multi-line token is changed, force the re-classication 
                            //of the whole multi-line token span and remove it from the list
                            _multiLineTokens.RemoveAt(i);
                            Invalidate(multiSpan);
                        }
                        else
                        {
                            //if no change, re-classify whole span with using current classification 
                            //(counterwise we loose actuale classification)
                            result.Add(new ClassificationSpan(multiSpan, _multiLineTokens[i].Classification));
                        }
                    }
                }
            }

            if (!isInsideMultiLine)
            {
                //Current span text
                string currentText = span.GetText();
                //Scan the current span for all tokens.

                ProcessLine(0, currentText, span, result);
            }

            return result;
        }

        private void ProcessLine(int shift, string line, SnapshotSpan span, List<ClassificationSpan> classifications)
        {
            int tokenStart = 0;
            SomeTokenList tokenList = new SomeTokenList();
            try
            {
                SomeCompiler.lexline(tokenList, 0, line.Substring(tokenStart));
            }
            catch { }

            for (SomeToken token = tokenList.First; token != null; token = token.NextToken)
            {
                if (token.Type == SomeTokenType.NewLine)
                    break;

                if (token.Type == SomeTokenType.ParenPair)
                {
                    //There might be something meaningful inside the parenthesis, e.g. a string constant
                    string inside = token.TokenString.Substring(1, token.TokenString.Length - 2);
                    ProcessLine(tokenStart + shift + token.ColumnStart + 1, inside, span, classifications);
                }
                else
                {
                    if(ProcessToken(tokenStart + shift, ref token, span, classifications))
                    {
                        break;
                    };
                }
            }
        }

        private bool ProcessToken(int shift, ref SomeToken token, SnapshotSpan span, List<ClassificationSpan> classifications)
        {
            int startIndex, endIndex;
            startIndex = span.Snapshot.GetLineFromLineNumber(token.LineNumber + span.Start.GetContainingLine().LineNumber).Start.Position + token.ColumnStart + shift;

            bool multiLine = false;
            IClassificationType clfType = GetClassificationType(span, ref token, ref multiLine);

            endIndex = span.Snapshot.GetLineFromLineNumber(token.LineNumber + span.Start.GetContainingLine().LineNumber).Start.Position + token.ColumnEnd + shift;

            if (endIndex > span.Snapshot.GetText().Length)
                endIndex = span.Snapshot.GetText().Length;

            if (clfType != null && startIndex <= endIndex)
            {
                SnapshotSpan ss = new SnapshotSpan(span.Snapshot, startIndex, endIndex - startIndex);
                ClassificationSpan clfSpan = new ClassificationSpan(ss, clfType);

                if (endIndex > startIndex && !span.Snapshot.TextBuffer.IsReadOnly(new Span(startIndex, endIndex - startIndex)))
                {
                    // Add the classfication span
                    classifications.Add(clfSpan);
                }

                //All multi-line tokens will be saved in a list and tracked. This will automatically
                //update the start / end position of token when text buffer is changed.
                if (multiLine)
                {
                    //Ensure that do not already exists into the list
                    if (!_multiLineTokens.Any(a => a.Tracking.GetSpan(span.Snapshot).Span == ss.Span))
                    {
                        _multiLineTokens.Add(new MultiLineToken()
                        {
                            Classification = clfType,
                            Version = span.Snapshot.Version,
                            Tracking =
                                span.Snapshot.CreateTrackingSpan(ss.Span,
                                                                 SpanTrackingMode.
                                                                     EdgeExclusive)
                        });

                        //If token length exeeed current span length, i need to invalidate and re-classify 
                        //the reaming text
                        if (ss.End > span.End)
                            Invalidate(new SnapshotSpan(span.End + 1, ss.End));
                    }
                }

            }

            return multiLine;
        }

        private IClassificationType GetClassificationType(SnapshotSpan span, ref SomeToken token, ref bool multiLine)
        {
            switch (token.Type)
            {
                case SomeTokenType.Blank:
                case SomeTokenType.Colon:
                case SomeTokenType.Comma:
                case SomeTokenType.Dot:
                case SomeTokenType.Greater:
                case SomeTokenType.LBracket:
                case SomeTokenType.LCurly:
                case SomeTokenType.Less:
                case SomeTokenType.NewLine:
                case SomeTokenType.None:
                case SomeTokenType.Percent:
                case SomeTokenType.RBracket:
                case SomeTokenType.RCurly:
                case SomeTokenType.Referrence:
                case SomeTokenType.Semi:
                case SomeTokenType.SnipEnd:
                case SomeTokenType.Snippet:
                case SomeTokenType.Tilde:
                    return classificationRegistryService.GetClassificationType(SomeClassificationTypes.Delimiter);
                case SomeTokenType.Equal:
                case SomeTokenType.CollectDef:
                    return classificationRegistryService.GetClassificationType(SomeClassificationTypes.Operator);
                case SomeTokenType.Digit:
                    return classificationRegistryService.GetClassificationType(SomeClassificationTypes.Number);
                case SomeTokenType.SingleLineComment:
                    return classificationRegistryService.GetClassificationType(SomeClassificationTypes.Comment);
                case SomeTokenType.SnipBegin:
                    while (token.Next != null && token.Type != SomeTokenType.SnipEnd)
                    {
                        token = token.Next;
                    }
                    if (token.Type != SomeTokenType.SnipEnd)
                    {
                        //continue to search
                        var tokenList = new SomeTokenList();
                        SomeCompiler.lex(tokenList, span.Snapshot.GetText().Substring(span.Start + 1));
                        token = tokenList.First;
                        while (token.Next != null && token.Type != SomeTokenType.SnipEnd)
                        {
                            token = token.Next;
                        }
                        multiLine = true;
                    }
                    return classificationRegistryService.GetClassificationType(SomeClassificationTypes.Snippet);
                case SomeTokenType.String:
                    return classificationRegistryService.GetClassificationType(SomeClassificationTypes.String);
                case SomeTokenType.Ident:
                    List<SomeClass> types = SomeCompiler.Classes;
                    SomeToken t = token;
                    if (types != null && types.Any(type => type.ClassName == t.TokenString))
                    {
                        return classificationRegistryService.GetClassificationType(SomeClassificationTypes.TypeName);
                    }
                    else if (SomeBasicTypes.IsBasicType(t.TokenString))
                    {
                        return classificationRegistryService.GetClassificationType(SomeClassificationTypes.BasicType);
                    }
                    else
                    {
                        return classificationRegistryService.GetClassificationType(SomeClassificationTypes.Identifier);
                    }
                default:
                    return classificationRegistryService.GetClassificationType(SomeClassificationTypes.Unknown);

            }
        }
    }
}