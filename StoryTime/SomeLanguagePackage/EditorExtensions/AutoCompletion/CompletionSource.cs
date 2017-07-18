using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using SOME.SomeLanguageCore.Tokens;
using SOME.SomeLanguageCore;
using SOME.SomeLanguageCore.CodeDom;
using SOME.SomeLanguageCore.Parser;

namespace SOME.SomeLanguageService.EditorExtensions.AutoCompletion
{
    /// <summary>
    /// Implementation of <see cref="ICompletionSource"/>. Provides the completion sets for the editor. 
    /// </summary>
    internal class SomeCompletionSource : ICompletionSource
    {

        private CompletionSourceProvider m_sourceProvider;
        private ITextBuffer m_textBuffer;
        private List<Completion> m_compList;

        public SomeCompletionSource(CompletionSourceProvider sourceProvider, ITextBuffer textBuffer)
        {
            m_sourceProvider = sourceProvider;
            m_textBuffer = textBuffer;
        }
        
        //populates completion list
        void ICompletionSource.AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            int position = session.GetTriggerPoint(session.TextView.TextBuffer).GetPosition(m_textBuffer.CurrentSnapshot);
            int line = m_textBuffer.CurrentSnapshot.GetLineNumberFromPosition(position);
            int column = position - m_textBuffer.CurrentSnapshot.GetLineFromPosition(position).Start.Position;

            string content = m_textBuffer.CurrentSnapshot.GetText();
            string currentLine = m_textBuffer.CurrentSnapshot.Lines.ElementAt(line).GetText();

            string fileExt = Path.GetExtension(this.m_textBuffer.GetFileName());
            List<string> strList = new List<string>();

            switch (SomeParser.ParsingStateType)
            {
                case EditingStateType.Nothing:
                    break;
                case EditingStateType.SmrExpectingBaseType:
                case EditingStateType.SmrExpectingType:
                    {
                        strList.AddRange(SomeBasicTypes.BasicTypeNames);
                        strList.AddRange(SomeCompiler.ClassNames);

                        m_compList = new List<Completion>();

                        foreach (string str in strList)
                        {
                            //exclude the type name from its base type names
                            if (SomeParser.ParsingStateType == EditingStateType.SmrExpectingBaseType)
                            {
                                string className = SomeParser.ParsingState.Context as string;
                                if (className == str)
                                {
                                    continue;
                                }
                            }
                            m_compList.Add(new Completion(str, str, str, null, null));
                        }
                    } break;
                case EditingStateType.SmrExpectingOverriden:
                case EditingStateType.SmrExpectingTypeOrOverriden:
                    {
                        if (SomeParser.ParsingStateType == EditingStateType.SmrExpectingTypeOrOverriden)
                        {
                            strList.AddRange(SomeBasicTypes.BasicTypeNames);
                            strList.AddRange(SomeCompiler.ClassNames);
                        }
                        SomeClass someCls = SomeParser.ParsingState.Context as SomeClass;
                        if(someCls == null)
                            break;

                        someCls.MethodsToOverride.ForEach(method =>
                            {
                                strList.Add("o_" + method.Name);
                                strList.Add("v_" + method.Name);
                            });

                        m_compList = new List<Completion>();
                        strList.ForEach(str => m_compList.Add(new Completion(str, str, str, null, null)));
                    } break;
                case EditingStateType.SmsExpectingObject:
                    {
                        SomeSequence seq = SomeParser.ParsingState.Context as SomeSequence;
                        List<string> objectNames = seq == null ? new List<string>() : seq.LocalsOrMemberObjects;
                        strList.AddRange(objectNames);

                        m_compList = new List<Completion>();
                        strList.ForEach(str => m_compList.Add(new Completion(str, str, str, null, null)));
                    } break;
                case EditingStateType.SmsExpectingDecendantType:
                    {
                        SomeClass type = SomeParser.ParsingState.Context as SomeClass;
                        List<string> ancestorNames = new List<string>();
                        if(type != null)
                        {
                            type.Decendants.ForEach(ancestor => ancestorNames.Add("<" + ancestor.ClassName + ">"));
                        }
                        strList.AddRange(ancestorNames);
                        m_compList = new List<Completion>();
                        strList.ForEach(str => m_compList.Add(new Completion(str, str, str, null, null)));
                    }break;
                case EditingStateType.SmsExpectingMember:
                    {
                        SomeClass someCls = SomeParser.ParsingState.Context as SomeClass;
                        if (someCls == null)
                            break;

                        someCls.Members.ForEach(member =>
                        {
                            strList.Add("." + member);
                        });

                        m_compList = new List<Completion>();
                        strList.ForEach(str => m_compList.Add(new Completion(str, str, str, null, null)));
                    }break;
                case EditingStateType.SmsExpectingObjectInContext:
                    {
                        object[] objs = SomeParser.ParsingState.Context as object[];
                        SomeSequence seq = objs[0] as SomeSequence;
                        List<string> objectNames = seq.LocalsOrMemberObjects;
                        SomeClass callingType = objs[1] as SomeClass;
                        objectNames.AddRange(callingType.MemberFields);
                        strList.AddRange(objectNames);

                        m_compList = new List<Completion>();
                        strList.ForEach(str => m_compList.Add(new Completion(str, str, str, null, null)));
                }break;
                case EditingStateType.SmsExpectingObjectOrMember:
                    {
                        SomeSequence seq = SomeParser.ParsingState.Context as SomeSequence;
                        List<string> objectOrTypes = seq == null ? new List<string>() : seq.LocalsOrMembers;

                        strList.AddRange(objectOrTypes);

                        m_compList = new List<Completion>();
                        strList.ForEach(str => m_compList.Add(new Completion(str, str, str, null, null)));
                    }break;
                case EditingStateType.SmsExpectingAny:
                    {
                        SomeSequence seq = SomeParser.ParsingState.Context as SomeSequence;
                        List<string> objectOrTypes = seq == null ? new List<string>() : seq.LocalsOrMembers;

                        strList.AddRange(objectOrTypes);

                        strList.AddRange(SomeBasicTypes.BasicTypeNames);
                        strList.AddRange(SomeCompiler.ClassNames);
                        
                        m_compList = new List<Completion>();
                        strList.ForEach(str => m_compList.Add(new Completion(str, str, str, null, null)));
                    }break;
                default:
                    break;
            }

            completionSets.Add(new CompletionSet(
                "Tokens",    //the non-localized title of the tab
                "Tokens",    //the display title of the tab
                FindTokenSpanAtPosition(session.GetTriggerPoint(m_textBuffer), session),
                m_compList,
                null));
        }

        private ITrackingSpan FindTokenSpanAtPosition(ITrackingPoint point, ICompletionSession session)
        {
            SnapshotPoint currentPoint = (session.TextView.Caret.Position.BufferPosition) - 1;
            ITextStructureNavigator navigator = m_sourceProvider.NavigatorService.GetTextStructureNavigator(m_textBuffer);
            TextExtent extent = navigator.GetExtentOfWord(currentPoint);
            return currentPoint.Snapshot.CreateTrackingSpan(extent.Span, SpanTrackingMode.EdgeInclusive);
        }


        private bool m_isDisposed;
        public void Dispose()
        {
            if (!m_isDisposed)
            {
                GC.SuppressFinalize(this);
                m_isDisposed = true;
            }
        }
    }
}