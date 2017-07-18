using System;
using System.Collections.Generic;
using System.Text;
using SOME.SomeLanguageCore.Tokens;
using SOME.SomeLanguageCore.CodeDom;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace SOME.SomeLanguageCore.Parser
{
    public class SomeParser
    {
        public static SomeClass ParseType(SomeToken solo, string nameSpace)
        {
            return ParseType(solo, solo, nameSpace);
        }

        [ThreadStatic]
        private static EditingState _parsingState = new EditingState(EditingStateType.Nothing, null);

        public static EditingState ParsingState
        {
            get { return _parsingState; }
        }

        public static EditingStateType ParsingStateType
        {
            get
            {
                return _parsingState.StateType;
            }
        }

        public static void EnterParsingState(EditingStateType stateType, object context)
        {
            _parsingState = new EditingState(stateType, context);

            //StackTrace st = new StackTrace(true);
            //for (int i = 0; i < st.FrameCount; i++)
            //{
            //    // Note that high up the call stack, there is only 
            //    // one stack frame.
            //    StackFrame sf = st.GetFrame(i);
            //    Debug.WriteLine("");
            //    Debug.WriteLine("High up the call stack, Method: {0}",
            //        sf.GetMethod());

            //    Debug.WriteLine("High up the call stack, Line Number: {0}",
            //        sf.GetFileLineNumber());
            //}

        }

        public static SomeClass ParseType(SomeToken begin, SomeToken end, string nameSpace)
        {
            EnterParsingState(EditingStateType.Nothing, null);

            //removeComment(begin, end);

            //first line: declaration
            //TestOutPut(begin, end);
            SomeClass someCls = new SomeClass(begin.TokenString, nameSpace);

            //just a name
            if (begin == end)
            {
                goto RETURN;
            }

            SomeToken token = begin.Next;

            int curLine = begin.LineNumber;
            if (token.LineNumber == begin.LineNumber)
            {
                SomeToken beginType = token;
                //base list and referrence list
                while (token != end && token.LineNumber == begin.LineNumber)
                {
                    token = token.Next;
                }
                SomeToken endType = token;
                parseTypeDeclareLine(beginType, endType, someCls);
            }


            //no members
            if (token == end)
            {
                goto RETURN;
            }

            //member lines
            curLine++;
            for (; token != end; curLine++)
            {
                SomeToken beginDec = token;
                for (; token != end && token.LineNumber == curLine; token = token.Next)
                {
                }

                SomeToken endDec = token;

                try
                {
                    parseMemberDeclarationLine(beginDec, endDec, someCls);
                }
                catch (Exception ex)
                {
                    SomeErrorReporter.AddError(ex.Message, curLine);
                }
            }

        RETURN:
            return someCls;
        }

        private static void parseTypeDeclareLine(SomeToken begin, SomeToken end, SomeClass someCls)
        {
            //parent list and reference list
            for (SomeToken token = begin; token != end; token = token.Next)
            {
                if (token.Type == SomeTokenType.Comma)
                {
                    EnterParsingState(EditingStateType.SmrExpectingBaseType, someCls.ClassName);
                }
                //parent types
                if (token.Type == SomeTokenType.Colon)
                {
                    EnterParsingState(EditingStateType.SmrExpectingBaseType, someCls.ClassName);
                    int lineNo = token.LineNumber;

                    token = token.Next;
                    while (token != null && token.Type == SomeTokenType.Blank)
                    {
                        token = token.Next;
                    }

                    if (token == end || token.Type != SomeTokenType.Ident)
                    {
                        SomeErrorReporter.AddError("parents expected after :", lineNo);
                        return;
                    }
                    someCls.AddParent(token.TokenString);
                }
                //parse referrence
                if (token.Type == SomeTokenType.Referrence)
                {
                    //->
                    EnterParsingState(EditingStateType.SmrExpectingType, null);
                    token = token.Next;

                    if (token == end || token.Type != SomeTokenType.Ident)
                    {
                        SomeErrorReporter.AddError("referrence type expected after ->", token.LineNumber);
                        return;
                    }

                    //Ident
                    SomeField field = new SomeField();
                    field.Type = token.TokenString;
                    someCls.AddField(field);

                    token = token.Next;
                    if (token == end || token.Type != SomeTokenType.LBracket)
                    {
                        SomeErrorReporter.AddError("'[' expected", token.LineNumber);
                        return;
                    }

                    //[
                    EnterParsingState(EditingStateType.Nothing, null);

                    token = token.Next;
                    if (token == end)
                    {
                        SomeErrorReporter.AddError("referrence name expected after '['", token.LineNumber);
                        return;
                    }

                    SomeToken refbegin = token;
                    SomeToken refEnd = token;

                    for (; token != end && token.Type != SomeTokenType.RBracket; token = token.Next)
                    {
                        refEnd = token;
                        if (token.Type == SomeTokenType.Less)
                            EnterParsingState(EditingStateType.SmrExpectingBaseType, field.Type);

                        if (token.Type == SomeTokenType.Greater)
                            EnterParsingState(EditingStateType.Nothing, null);
                    }

                    if (token == end)
                    {
                        SomeErrorReporter.AddError("']' expected", token.LineNumber);
                        return;
                    }

                    //type name
                    parseFieldName(refbegin, refEnd, someCls, field);

                }//referrence
            }
        }

        //remove commentary tokens
        private static void removeComment(SomeToken begin, SomeToken end)
        {
            for (SomeToken token = begin; token != null; token = token.NextToken)
            {
                if (token.Type == SomeTokenType.SingleLineComment)
                {
                    if (token.NextToken == null)
                    {
                        token = null;
                        break;
                    }
                    else
                    {
                        token.LineNumber = token.NextToken.LineNumber;
                        token.TokenString = token.NextToken.TokenString;
                        token.Type = token.NextToken.Type;
                        token.NextToken = token.NextToken.NextToken;
                    }
                }
            }
        }

        private static void parseFieldName(SomeToken begin, SomeToken end, SomeClass cls, SomeField field)
        {
            SomeToken token = begin;
            SomeToken rBrackt = end.Next;

            if (token.Type != SomeTokenType.Ident)
            {
                SomeErrorReporter.AddError("cannot parse field", token.LineNumber);
                return;
            }

            //Ident

            parseMemberNameIntoField(token.TokenString, field, cls);

            token = token.Next;

            if (token == rBrackt)
            {
                //end of referrence name
                return;
            }

            if (token.Type == SomeTokenType.Dot)
            {
                field.IsInitialized = true;
                token = token.Next;
                if (token == rBrackt || token.Type != SomeTokenType.ParenPair)
                {
                    SomeErrorReporter.AddError("() expected.", token.LineNumber);
                    return;
                }
                //(...)
                parseReferrenceFieldCreation(token, field);
            }
            else if (token.Type == SomeTokenType.Less)
            {
                //<
                token = token.Next;
                if (token == rBrackt || token.Type != SomeTokenType.Ident)
                {
                    SomeErrorReporter.AddError("cannot parse field.", token.LineNumber);
                    return;
                }

                //Ident

                field.IsInitialized = true;
                field.InitialCreationType = token.TokenString;
                token = token.Next;

                if (token == rBrackt || token.Type != SomeTokenType.Greater)
                {
                    SomeErrorReporter.AddError("'>' is expected.", token.LineNumber);
                    return;
                }
                //>
                token = token.Next;
                if (token == null || token.Type != SomeTokenType.Dot)
                {
                    SomeErrorReporter.AddError("'.' is expected", token.LineNumber);
                    return;
                }
                //.
                token = token.Next;
                if (token == null || token.Type != SomeTokenType.ParenPair)
                {
                    SomeErrorReporter.AddError("cannot parse field.", token.LineNumber);
                    return;
                }
                parseReferrenceFieldCreation(token, field);
            }
            else
            {
                SomeErrorReporter.AddError("cannot parse field.", token.LineNumber);
                return;
            }

            token = token.Next;

            if (token != rBrackt)
            {
                SomeErrorReporter.AddError("cannot parse field.", token.LineNumber);
                return;
            }
        }

        private static void parseMemberNameIntoField(string ident, SomeField fld, SomeClass cls)
        {
            //private member
            if (ident[0] == SomeChar.ULine)
            {
                fld.Name = ident;
                return;
            }
            //property 
            if (char.IsUpper(ident[0]))
            {
                fld.Name = "_" + MakeFirstLower(ident);
                SomeProperty prop = new SomeProperty(fld);
                cls.AddPropery(prop);
                return;
            }
            if (ident.Length >= 3 && ident[1] == SomeChar.ULine)
            {
                switch (ident[0])
                {
                    case 'm':
                        {
                            fld.AccLevel = AccessLevel.Protected;
                            fld.Name = ident;
                        } break;
                    case 's':
                        {
                            fld.IsStatic = true;
                            fld.Name = "_" + MakeFirstLower(ident.Substring(2));
                            if (char.IsUpper(ident[2]))
                            {
                                //static property
                                SomeProperty prop = new SomeProperty(fld);
                                cls.AddPropery(prop);
                            }
                        } break;
                    case 'c':
                        {
                            fld.IsConst = true;
                            fld.Name = "_" + MakeFirstLower(ident.Substring(2));
                            if (char.IsUpper(ident[2]))
                            {
                                SomeProperty prop = new SomeProperty(fld);
                                cls.AddPropery(prop);
                            }
                        } break;
                    case 'r':
                        {
                            //read-only property
                            fld.Name = "_" + MakeFirstLower(ident.Substring(2));
                            SomeProperty prop = new SomeProperty(fld);
                            prop.IsSettable = false;
                            cls.AddPropery(prop);
                        } break;
                    case 'w':
                        {
                            //write-only property
                            fld.Name = "_" + MakeFirstLower(ident.Substring(2));
                            SomeProperty prop = new SomeProperty(fld);
                            prop.IsGettable = false;
                            cls.AddPropery(prop);
                        } break;

                    default:
                        break;
                }
                return;
            }

            fld.Name = ident;
        }

        private static void parseMemberDeclarationLine(SomeToken begin, SomeToken end, SomeClass someCls)
        {
            SomeToken token = begin;
            if (token.Type == SomeTokenType.Ident)
            {
                //_, v_
                if (token.TokenString == "_" || token.TokenString == "v_")
                {
                    //private or protected constructor
                    SomeMethod method = new SomeMethod();
                    method.IsConstructor = true;
                    method.AccLevel = token.TokenString == "_" ? AccessLevel.Private : AccessLevel.Protected;
                    method.Name = someCls.ClassName;
                    someCls.AddMethod(method);


                    token = token.Next;
                    if (token == end || token.Type != SomeTokenType.ParenPair)
                    {
                        ParenStateTransitionSmr(token);
                        SomeErrorReporter.AddError("() are expected.", token.LineNumber);
                        return;
                    }
                    //_(),v_()
                    parseMethodDeclareParaList(token, method, someCls);
                }
                //_xxx
                else if (token.TokenString[0] == SomeChar.ULine)
                {
                    //private void method
                    SomeMethod method = new SomeMethod();
                    method.Name = token.TokenString;
                    someCls.AddMethod(method);

                    token = token.Next;
                    if (token == end || token.Type != SomeTokenType.ParenPair)
                    {
                        ParenStateTransitionSmr(token);
                        SomeErrorReporter.AddError("() are expected.", token.LineNumber);
                        return;
                    }
                    //_xxx()
                    parseMethodDeclareParaList(token, method, someCls);
                    EnterParsingState(EditingStateType.SmrExpectingType, null);
                }
                //x
                else if (token.TokenString.Length == 1)
                {
                    if (token.TokenString == "v" || token.TokenString == "o")
                    {
                        EnterParsingState(EditingStateType.SmrExpectingTypeOrOverriden, someCls);
                    }
                    else
                    {
                        EnterParsingState(EditingStateType.SmrExpectingType, null);
                    }
                }
                //x_xxx
                else if (token.TokenString[1] == SomeChar.ULine)
                {
                    //special void function
                    SomeMethod method = new SomeMethod();
                    someCls.AddMethod(method);

                    SomeToken ident = token;
                    token = token.Next;
                    if (token == end || token.Type != SomeTokenType.ParenPair)
                    {
                        ParenStateTransitionSmr(token);
                        SomeErrorReporter.AddError("() are expected.", token.LineNumber);
                        return;
                    }
                    //x_xxx()
                    SomeToken parens = token;
                    parseSpecialMethod(ident, parens, method, someCls);
                    EnterParsingState(EditingStateType.SmrExpectingType, null);
                }
                //xxx
                else
                {
                    //a type or a normal function
                    if (token.Next == end)
                    {
                        if (token.TokenString == "main")
                        {
                            SomeMethod method = new SomeMethod();
                            someCls.AddMethod(method);
                            method.AccLevel = AccessLevel.Public;
                            method.IsStatic = true;
                            method.Name = "main";
                        }
                        else
                        {
                            SomeErrorReporter.AddError(string.Format("cannot parse '{0}'", token.TokenString), token.LineNumber);
                        }
                        return;
                    }
                    if (token.Next.Type == SomeTokenType.ParenPair)
                    {
                        //xxx(), normal void function
                        SomeMethod method = new SomeMethod();
                        method.Name = token.TokenString;
                        if (char.IsUpper(method.Name[0]))
                        {
                            method.AccLevel = AccessLevel.Public;
                        }
                        else
                        {
                            method.AccLevel = AccessLevel.Protected;
                        }

                        someCls.AddMethod(method);
                        token = token.Next;
                        //(...)
                        parseMethodDeclareParaList(token, method, someCls);
                        EnterParsingState(EditingStateType.SmrExpectingType, null);
                    }
                    //xxx ident
                    else if (token.Next.Type == SomeTokenType.Ident)
                    {
                        EnterParsingState(EditingStateType.Nothing, null);
                        //xxx ident(), function
                        if (token.Next.Next != end && token.Next.Next.Type == SomeTokenType.ParenPair)
                        {
                            //xxx yyy()
                            SomeMethod method = new SomeMethod();
                            method.Type = token.TokenString;
                            someCls.AddMethod(method);
                            token = token.Next;

                            //xxx _yyy()
                            if (token.TokenString[0] == SomeChar.ULine)
                            {
                                //private method name
                                method.Name = token.TokenString;

                                token = token.Next;
                                if (token == end || token.Type != SomeTokenType.ParenPair)
                                {
                                    ParenStateTransitionSmr(token);
                                    SomeErrorReporter.AddError("() are expected.", token.LineNumber);
                                    return;
                                }
                                //(...)
                                parseMethodDeclareParaList(token, method, someCls);
                                EnterParsingState(EditingStateType.SmrExpectingType, null);
                            }
                            //xxx y_yyy()
                            else if (token.TokenString[1] == SomeChar.ULine)
                            {
                                //special function
                                SomeToken ident = token;
                                token = token.Next;
                                if (token == end || token.Type != SomeTokenType.ParenPair)
                                {
                                    ParenStateTransitionSmr(token);
                                    SomeErrorReporter.AddError("() are expected.", token.LineNumber);
                                    return;
                                }
                                SomeToken parens = token;
                                parseSpecialMethod(ident, parens, method, someCls);
                                EnterParsingState(EditingStateType.SmrExpectingType, null);
                            }
                            //xxx yyy()
                            else
                            {
                                //normal function
                                method.Name = token.TokenString;
                                if (char.IsUpper(method.Name[0]))
                                {
                                    method.AccLevel = AccessLevel.Public;
                                }
                                else
                                {
                                    method.AccLevel = AccessLevel.Protected;
                                }

                                token = token.Next;
                                //(...)
                                parseMethodDeclareParaList(token, method, someCls);
                                EnterParsingState(EditingStateType.SmrExpectingType, null);
                            }
                        }
                        //xxx ident, field
                        else
                        {
                            SomeField field = new SomeField();
                            field.Type = token.TokenString;
                            token = token.Next;

                            //xxx y
                            if (token.TokenString == "v" || token.TokenString == "o")
                            {
                                EnterParsingState(EditingStateType.SmrExpectingOverriden, someCls);
                            }

                            SomeToken fldBegin = token;
                            SomeToken fldEnd = token;
                            for (; token != end; token = token.Next)
                            {
                                fldEnd = token;
                            }
                            parseFieldName(fldBegin, fldEnd, someCls, field);
                            token = fldEnd;
                            someCls.AddField(field);
                        }
                    }
                    else
                    {
                        SomeErrorReporter.AddError("cannot parse member declaration.", token.LineNumber);
                        return;
                    }
                }
            }
            //()
            else if (token.Type == SomeTokenType.ParenPair)
            {
                //(...), public constructor
                SomeMethod method = new SomeMethod();
                method.IsConstructor = true;
                method.AccLevel = AccessLevel.Public;
                method.Name = someCls.ClassName;
                someCls.AddMethod(method);

                parseMethodDeclareParaList(token, method, someCls);
                EnterParsingState(EditingStateType.SmrExpectingType, null);
            }
            //~
            else if (token.Type == SomeTokenType.Tilde)
            {
                //destructor
                SomeMethod method = new SomeMethod();
                method.IsDestructor = true;
                method.Name = SomeChar.Tilde.ToString() + someCls.ClassName;
                someCls.AddMethod(method);

                token = token.Next;
                if (token == end || token.Type != SomeTokenType.ParenPair)
                {
                    SomeErrorReporter.AddError("() are expected.", token.LineNumber);
                    return;
                }
                //~()
            }
            else if (token.Type == SomeTokenType.Unknown) //could be only a "("
            {
                ParenStateTransitionSmr(token);
            }
        }


        /// <summary>
        /// used for AutoCompletion. determine when to enable intellisense list when typing after a "("
        /// </summary>
        private static void ParenStateTransitionSmr(SomeToken next)
        {
            if (next != null && next.TokenString.StartsWith(SomeChar.LParen.ToString()))
            {
                EnterParsingState(EditingStateType.SmrExpectingType, null);
                //analyse the rest part
                string rest = next.TokenString.Substring(1);
                string[] paras = rest.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string paraStr in paras)
                {
                    string[] paraDeclares = paraStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (paraDeclares.Length == 2)
                    {
                        EnterParsingState(EditingStateType.Nothing, null);
                    }
                    if (paraDeclares.Length == 1)
                    {
                        EnterParsingState(EditingStateType.SmrExpectingType, null);
                    }
                }
            }
        }

        private static void ParenStateTransitionSms(SomeToken next, SomeSequence parentSeq, SomeClass methodType)
        {
            /* (obj1, ...)
             * (m_obj1 = obj1, ...)
             * (obj1[obj2], ...)
            */
            if (next.TokenString.StartsWith(SomeChar.LParen.ToString()))
            {
                EnterParsingState(EditingStateType.SmsExpectingObject, parentSeq);

                //analyse the rest part
                string rest = next.TokenString.Substring(1);
                string[] paras = rest.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string paraStr in paras)
                {
                    string[] paraDeclares = paraStr.Split(new char[] { '=', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    if (paraDeclares.Length == 2)
                    {
                        if (paraStr.Contains("="))
                        {
                            EnterParsingState(EditingStateType.SmsExpectingObject, parentSeq);
                        }
                        else
                        {
                            EnterParsingState(EditingStateType.Nothing, null);
                        }
                    }
                    if (paraDeclares.Length == 1)
                    {
                        EnterParsingState(EditingStateType.SmsExpectingObjectInContext, new object[]{parentSeq, methodType});
                    }
                }
            }
        }

        private static void parseMethodDeclareParaList(SomeToken parenPair, SomeMethod method, SomeClass cls)
        {
            if (parenPair.TokenString.Replace(" ", "") != "()")
            {
                string paraListStr = parenPair.TokenString;
                paraListStr = paraListStr.Substring(1, paraListStr.Length - 2).Trim();
                string[] paras = paraListStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string paraStr in paras)
                {
                    string[] paraDeclares = paraStr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    SomeParameter paraMeter = new SomeParameter();
                    paraMeter.Type = paraDeclares[0];

                    if (paraDeclares.Length >= 2)
                    {
                        if (paraDeclares[1][0] == SomeChar.ULine)
                        {
                            paraMeter.Name = MakeFirstLower(paraDeclares[1].Substring(1));
                        }
                        else if (paraDeclares[1].Length >= 3 && paraDeclares[1][1] == SomeChar.ULine)
                        {
                            paraMeter.Name = MakeFirstLower(paraDeclares[1].Substring(2));
                        }
                        else
                        {
                            paraMeter.Name = MakeFirstLower(paraDeclares[1]);
                        }

                        //another field
                        SomeField field = new SomeField();
                        field.Type = paraMeter.Type;
                        parseMemberNameIntoField(paraDeclares[1], field, cls);
                        cls.AddField(field);
                    }
                    method.AddParameter(paraMeter);
                }
            }
            //no parameters
        }

        public static string MakeFirstUpper(string s)
        {
            if (s.Length == 1)
                return s.ToUpper();
            else
                return s[0].ToString().ToUpper() + s.Substring(1);
        }

        public static string MakeFirstLower(string ident)
        {
            if (ident.Length == 1)
                return ident.ToLower();
            else
                return ident[0].ToString().ToLower() + ident.Substring(1);
        }

        private static void parseReferrenceFieldCreation(SomeToken ParenPair, SomeField fld)
        {
            string initList = ParenPair.TokenString;
            fld.InitialCreationParaString = initList.Substring(1, initList.Length - 2);
        }

        private static void parseSpecialMethod(SomeToken ident, SomeToken parens, SomeMethod method, SomeClass cls)
        {
            SomeToken token = ident;
            method.Name = token.TokenString.Substring(2);
            if (char.IsUpper(method.Name[0]))
            {
                method.AccLevel = AccessLevel.Public;
            }
            switch (token.TokenString[0])
            {
                case 'a':
                    method.IsAbstract = true;
                    break;
                case 'v':
                    method.IsVirtual = true;
                    break;
                case 'o':
                    method.IsOverride = true;
                    break;
                case 's':
                    method.IsStatic = true;
                    break;
                default:
                    SomeErrorReporter.AddError(string.Format("cannot parse '{0}'.", token.TokenString), token.LineNumber);
                    break;
            }

            parseMethodDeclareParaList(parens, method, cls);
        }

        //Parsing Sequence into CodeDom, none recursive, the sequence is not necessarily ended (still open)
        public static void ParseSequence2(ref SomeToken cursor, SomeToken end, SomeSequence parentSeq, SomeDom codeDom)
        {
            //loop through every statement or sub sequence
            SomeToken state_begin = cursor;
            for (; cursor != end && cursor != null; cursor = cursor.Next)
            {
                if (
                    cursor.Type == SomeTokenType.Semi ||
                    cursor.Type == SomeTokenType.Snippet ||
                    cursor.Type == SomeTokenType.SingleLineComment
                    )
                {
                    //ignore excessive ";"
                    if (state_begin.Type == SomeTokenType.Semi)
                    {
                        state_begin = cursor.Next;
                        continue;
                    }

                    if (parentSeq == null)
                    {
                        SomeErrorReporter.AddError("cannot parse sequence, no parent sequence for this statement.", cursor.LineNumber);
                        return;
                    }
                    else
                    {
                        SomeStatement statement = parseStatement(state_begin, cursor, parentSeq);
                        parentSeq.AddStatement(statement);
                        state_begin = cursor.Next;
                    }
                }
                else if (cursor.Type == SomeTokenType.LCurly)
                {
                    SomeStatement statement = parseStatement(state_begin, cursor, parentSeq);
                    SomeSequence someSeq = new SomeSequence();
                    someSeq.AttachedStatement = statement;
                    if (parentSeq != null)
                    {
                        parentSeq.AddStatement(statement);
                        parentSeq.AddSubSequence(someSeq);
                    }
                    else
                    {
                        //Top sequence
                        codeDom.AddSequence(someSeq);
                    }

                    cursor = cursor.Next;
                    ParseSequence2(ref cursor, end, someSeq, codeDom);

                    //cursor could be "}" or end (unfinished sub sequence)
                    if (cursor == end)
                    {
                        return;
                    }

                    state_begin = cursor.Next;
                }
                else if (cursor.Type == SomeTokenType.RCurly)
                {
                    //the current sequence ends properly
                    return;
                }
            }

            if (state_begin != null && state_begin.Type != SomeTokenType.Semi)
            {
                //the sequence is ended in advance, must be in editing, time to bring up completion list
                HandleUnfinishedSequence(state_begin, end, parentSeq, codeDom);
            }

            SomeErrorReporter.AddError("} expected for this sequence", cursor.LineNumber);
        }

        private static void HandleUnfinishedSequence(SomeToken begin, SomeToken end, SomeSequence seq, SomeDom codeDom)
        {
            codeDom.ParseMethods();
            parseStatement(begin, end, seq);
        }

        //Unfinished alternative solution
        public static void ParseSequence3(SomeToken begin, SomeToken end, SomeDom codeDom)
        {
            //SomeTokenList statementTokens = new SomeTokenList();
            //SomeToken statementStart = begin;
            //SomeToken lastToken = null;

            //while (true)
            //{
            //    if (begin == end)
            //    {
            //        //if last token is reached, check Auto Completion status
            //        if (begin.Type != SomeTokenType.RCurly)
            //        {
            //        }

            //        //end of parsing
            //        break;
            //    }

            //    if (statementStart.LineNumber != begin.LineNumber)
            //    {
            //        //if line has changed, create an error report and reset statementStart, and the last line will be ignored
            //        SomeErrorReporter.AddError("; expected", begin.LineNumber);
            //        statementStart = begin;
            //    }

            //    switch (begin.Type)
            //    {
            //        case SomeTokenType.LCurly:
            //            {
            //                codeDom.AddMethodCall(ParseStatement(statementStart, lastToken));
            //                statementStart = begin.Next;
            //            }break;
            //        case SomeTokenType.Semi:
            //            {
            //                codeDom.AddStatement(ParseStatement(statementStart, lastToken));
            //                statementStart = begin.Next;
            //            }break;
            //        case SomeTokenType.Snippet:
            //            {
            //            }break;
            //        case SomeTokenType.SingleLineComment:
            //            {
            //            }break;

            //    }

            //    lastToken = begin;
            //    begin = begin.Next;
            //}
        }

        public static SomeStatement ParseStatement(SomeToken begin, SomeToken end)
        {
            SomeStatement statement = null;
            if (end.Type == SomeTokenType.Snippet)
            {
            }

            return statement;
        }

        //public static SomeStatementComment ParseSnippetStatement(SomeToken snippet)
        //{
        //}

        //public static SomeStatementComment PraseSingleLineCommentStatement(SomeToken comment)
        //{
        //}

        //TODOZ: to be obsolete
        public static SomeSequence ParseSequence(SomeToken begin, SomeToken end, SomeSequence parentSeq)
        {
            SomeSequence someSeq = new SomeSequence(begin, end);
            SomeToken token = begin;
            SomeToken invoke_begin = token;
            for (; token != end && token.Type != SomeTokenType.LCurly; token = token.Next)
            {
            }

            if (token == end)
            {
                SomeErrorReporter.AddError("cannot parse sequence, '{' is expected.", begin.LineNumber);
                return null;
            }

            SomeToken invoke_end = token;

            SomeStatement statement = parseStatement(invoke_begin, invoke_end, parentSeq);
            if (parentSeq != null)
            {
                parentSeq.AddStatement(statement);
            }
            someSeq.AttachedStatement = statement;

            //{
            token = token.Next;
            for (; token != end; token = token.Next)
            {
                SomeToken state_begin = token;
                SomeToken state_end = token;
                for (int curly = 0; token != end; token = token.Next)
                {
                    //if (paren == 1 && token.Type == TokenType.Snippet)
                    //{
                    //    someSeq.AddStatement(new SomeStatementSnippet(token.TokenString));
                    //    continue;
                    //}

                    state_end = token;
                    if (curly == 0 &&
                        (
                        token.Type == SomeTokenType.Semi ||
                        token.Type == SomeTokenType.Snippet ||
                        token.Type == SomeTokenType.SingleLineComment)
                        )
                    {
                        break;
                    }
                    if (token.Type == SomeTokenType.LCurly)
                    {
                        curly++;
                    }
                    if (token.Type == SomeTokenType.RCurly)
                    {
                        if (curly == 0)
                        {
                            //end of this sequence
                            goto RETURN;
                        }
                        curly--;
                    }
                }
                if (token == end)
                {
                    //Error,no semicolon found
                    SomeErrorReporter.AddError(string.Format("{0}: cannot parse sequence, ';' is expected.", state_begin.LineNumber));
                    goto RETURN;
                }
                //;
                parseStatementOrSequence(state_begin, state_end, someSeq);
            }
        RETURN:
            return someSeq;
        }

        private static void combineComparationPair(SomeToken begin, SomeToken end)
        {
        }

        public static void CombineSnippet(SomeToken begin, SomeToken end)
        {
            for (SomeToken token = begin; token != end; token = token.NextToken)
            {
                if (token.Type == SomeTokenType.SnipBegin)
                {
                    SomeToken snippet = token;
                    token = token.NextToken;

                    if (token == end)
                    {
                        //Error
                        SomeErrorReporter.AddError(string.Format("{0}: '%>' is expected.", snippet.LineNumber));
                        return;
                    }

                    StringBuilder sb = new StringBuilder();

                    int curLine = token.LineNumber;
                    for (; token != end && token.Type != SomeTokenType.SnipEnd; token = token.NextToken)
                    {
                        if (token.LineNumber != curLine)
                        {
                            //line changed
                            for (int i = 0; i < token.LineNumber - curLine; i++)
                            {
                                sb.AppendLine();
                            }
                            curLine = token.LineNumber;
                        }
                        sb.Append(token.TokenString);
                    }

                    if (token == end)
                    {
                        //if (token.Type != TokenType.SnipEnd)
                        //{
                        SomeErrorReporter.AddError(string.Format("{0}: '%>' is expected.", snippet.LineNumber));
                        //}
                        return;
                    }
                    snippet.Type = SomeTokenType.Snippet;
                    snippet.TokenString = SomeTokenTypeString.SnippetBegin + sb.ToString() + SomeTokenTypeString.SnippetEnd;

                    //skip
                    snippet.NextToken = token.NextToken;
                }
            }
        }

        private static string combineTokens(SomeToken begin, SomeToken end)
        {
            StringBuilder sb = new StringBuilder();
            bool last_ident = false;
            for (SomeToken token = begin; token != end.NextToken; token = token.NextToken)
            {
                sb.Append(token.TokenString);
                last_ident = (token.Type == SomeTokenType.Ident);
            }
            return sb.ToString();
        }

        //maybe a normal statement, or maybe a invoking with a sequence, or maybe a snippet
        private static void parseStatementOrSequence(SomeToken begin, SomeToken end, SomeSequence seq)
        {
            SomeToken token = begin;

            while (token.Type == SomeTokenType.EmptyLine)
            {
                token = token.Next;
            }

            if (token.Type == SomeTokenType.Snippet)
            {
                SomeStatement snippet = new SomeStatementSnippet(token.TokenString);
                seq.AddStatement(snippet);
                return;
            }

            if (token.Type == SomeTokenType.SingleLineComment)
            {
                SomeStatement comment = new SomeStatementComment(token.TokenString);
                seq.AddStatement(comment);
                return;
            }


            SomeToken beginToken = token;
            SomeToken endToken = token;
            for (; token != end && token.Type != SomeTokenType.LCurly; token = token.Next)
            {
            }

            if (token.Type == SomeTokenType.LCurly)
            {
                //add a sub sequence
                SomeSequence sub_seq = ParseSequence(beginToken, end, seq);
                seq.AddSubSequence(sub_seq);
                //seq.AddStatement(parseStatement(beginToken, end, sub_seq, seq));
            }
            else if (token.Type == SomeTokenType.Semi)
            {
                //;
                seq.AddStatement(parseStatement(beginToken, end, seq));
            }
        }

        public static void TestOutPut(SomeToken begin, SomeToken end)
        {
            for (SomeToken token = begin; token != end; token = token.Next)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(token.LineNumber.ToString()).Append(" ").Append(token.TokenString).Append(" ").Append(token.Type.ToString());
                SomeErrorReporter.AddError(sb.ToString());
                sb = null;
            }
            SomeErrorReporter.AddError("");
        }

        //the end should be either ";" or "{"
        private static SomeStatement parseStatement(SomeToken begin, SomeToken end, /*SomeSequence seq,*/ SomeSequence parent_seq)
        {
            SomeStatement statement = null;
            SomeToken token = begin;

            while (token.Type == SomeTokenType.EmptyLine && token != end)
            {
                token = token.Next;
            }

            if (token.Type == SomeTokenType.Snippet)
            {
                SomeStatement snippet = new SomeStatementSnippet(token.TokenString);
                return snippet;
            }

            if (token.Type == SomeTokenType.SingleLineComment)
            {
                SomeStatement comment = new SomeStatementComment(token.TokenString);
                return comment;
            }

            //check
            if (end.Type != SomeTokenType.Semi && end.Type != SomeTokenType.LCurly)
            {
                //could be in editing, hold off throwing exception for now
                //throw new SomeCompilerException("cannot parse statement, left curly or ';' is expected.", begin.LineNumber);
            }

            if (tryParseCreationStatement(token, end,/* seq,*/ parent_seq, ref statement)) return statement;
            if (tryParseInvokingStatement(token, end,/* seq,*/ parent_seq, ref statement, SomeTokenType.Unknown)) return statement;
            if (tryParseAssignmentStatement(token, end,/* seq,*/ parent_seq, ref statement)) return statement;
            if (tryParseReturnStatement(token, end, /*seq,*/ parent_seq, ref statement)) return statement;

            //throw new SomeCompilerException("cannot parse statement.", begin.LineNumber);
            SomeErrorReporter.AddError("Cannot parse statement", begin.LineNumber);
            return null;
        }

        private static bool tryParseCreationStatement(SomeToken begin, SomeToken end, /*SomeSequence seq,*/ SomeSequence parent_seq, ref SomeStatement statement)
        {
            SomeToken token = begin;
            if (token.Type == SomeTokenType.Ident)
            {
                string ident1 = token.TokenString;

                //Ident

                if (token == end)
                {
                    EnterParsingState(EditingStateType.SmsExpectingAny, parent_seq);
                    return true;
                }

                token = token.Next;

                if (token.Type == SomeTokenType.Ident) //Ident Ident
                {
                    //Defination
                    string ident2 = token.TokenString;

                    SomeExpressionDefination def = new SomeExpressionDefination();
                    def.DefinationType = ident1;
                    def.DefinationObject = ident2;

                    if (token == end)
                    {
                        EnterParsingState(EditingStateType.Nothing, null);
                        return true;
                    }

                    token = token.Next;

                    if (token.Type == SomeTokenType.Semi)
                    {
                        //Ident Ident;
                        statement = new SomeStatementCreation(def, null, SomeStatementCreation.SomeStatementCreationType.Defination);
                        return true;
                    }

                    return parseConstructionStatement(def, ident2, token, end, /*seq,*/ parent_seq, ref statement);
                }
                else if (token.Type == SomeTokenType.Dot)
                {
                    //Ident.
                    if (token == end)
                    {
                        EnterParsingState(EditingStateType.SmsExpectingMember, lookupObjTypeFromSequence(parent_seq, ident1));
                        return true;
                    }

                    if (SomeCompiler.IsTypeName(ident1))
                    {
                        return false;
                    }
                }
                //Construction
                return parseConstructionStatement(null, ident1, token, end, /*seq,*/ parent_seq, ref statement);
            }

            return false;
        }

        private static SomeClass lookupObjTypeFromSequence(SomeSequence seq, string objName)
        {
            string typeName = lookupObjTypeNameFromSequence(seq, objName);
            return SomeCompiler.Classes.Find(cls => cls.ClassName == typeName);
        }

        private static bool parseConstructionStatement(SomeExpressionDefination def, string obj, SomeToken begin, SomeToken end, /*SomeSequence seq,*/ SomeSequence parent_seq, ref SomeStatement statement)
        {
            SomeClass callingtype;
            if (def != null)
            {
                callingtype = SomeCompiler.lookupTypeFromName(def.DefinationType);
            }
            else
            {
                callingtype = lookupObjTypeFromSequence(parent_seq, obj);
            }

            SomeToken token = begin;
            if (token.Type == SomeTokenType.Dot)//... Ident.
            {
                if (token == end)
                {
                    if (def != null)   //Ident Ident.
                    {
                        //Ident Ident.
                        EnterParsingState(EditingStateType.Nothing, null);
                    }
                    return true;
                }

                token = token.Next;
                if (token.Type != SomeTokenType.ParenPair)
                {
                    if (token == end)
                    {
                        ParenStateTransitionSms(token, parent_seq, callingtype);
                        return true;
                    }

                    return false;
                }

                //Ident Ident.(...)
                SomeExperessionConstruction construct = new SomeExperessionConstruction();
                construct.ConstructionObject = obj;

                if (def != null)
                {
                    construct.ConstructionObjectType = def.DefinationType;
                    statement = new SomeStatementCreation(def, construct, SomeStatementCreation.SomeStatementCreationType.DefinationWithConstruction);
                }
                else
                {
                    construct.ConstructionObjectType = lookupObjTypeNameFromSequence(parent_seq, construct.ConstructionObject);
                    statement = new SomeStatementCreation(null, construct, SomeStatementCreation.SomeStatementCreationType.Construction);
                }

                return parseStatementInvokingParaList(construct.ConstructionObjectType, construct.ConstructionObjectType, token.TokenString, /*seq,*/ parent_seq, statement, token.LineNumber);
            }
            else if (token.Type == SomeTokenType.Less)//Ident Ident<
            {
                if (token == end)
                {
                    EnterParsingState(EditingStateType.SmsExpectingDecendantType, callingtype);
                    return true;
                }

                token = token.Next;
                if (token.Type != SomeTokenType.Ident)
                {
                    return false;
                }
                //Ident Ident<Ident
                string ident3 = token.TokenString;

                token = token.Next;
                if (token.Type != SomeTokenType.Greater)
                {
                    return false;
                }
                //Ident Ident<Ident>

                token = token.Next;
                if (token.Type != SomeTokenType.Dot)
                {
                    return false;
                }

                if (token == end)
                {
                    if (def != null)   //Ident Ident.
                    {
                        //Ident Ident.
                        EnterParsingState(EditingStateType.Nothing, null);
                    }
                    return true;
                }

                //Ident Ident<Ident>.
                token = token.Next;
                if (token.Type != SomeTokenType.ParenPair)
                {
                    if (token == end)
                    {
                        ParenStateTransitionSms(token, parent_seq, callingtype);
                        return true;
                    }

                    return false;
                }
                //Ident Ident<Ident>.(...)

                SomeExperessionConstruction construct = new SomeExperessionConstruction();
                construct.ConstructionObject = obj;
                if (def != null)
                {
                    construct.ConstructionObjectType = def.DefinationType;
                    construct.ConstructionObjectRealType = ident3;
                    statement = new SomeStatementCreation(def, construct, SomeStatementCreation.SomeStatementCreationType.DefinationWithConstruction);
                }
                else
                {
                    construct.ConstructionObjectType = lookupObjTypeNameFromSequence(parent_seq, construct.ConstructionObject);
                    construct.ConstructionObjectRealType = ident3;
                    statement = new SomeStatementCreation(null, construct, SomeStatementCreation.SomeStatementCreationType.Construction);
                }

                //statement = new SomeStatement(SomeStatementType.Creation);
                return parseStatementInvokingParaList(construct.ConstructionObjectRealType, construct.ConstructionObjectRealType, token.TokenString, /*seq,*/ parent_seq, statement, token.LineNumber);
            }

            return false;
        }

        private static bool parseStatementInvokingParaList(
            string inv_type,
            string inv_mthd,
            string paren_pair,
            /*SomeSequence seq,*/
            SomeSequence parent_seq,
            SomeStatement statement,
            int line_number
            )
        {
            /* (obj1)
             * (obj1,obj2)
             * (m_obj1 = obj1)
             * (obj1[obj2])
             */
            string[] div_byparen = paren_pair.Split(new char[] { SomeChar.LParen, SomeChar.RParen });

            //get rid of the parens pair
            string para_list = div_byparen[1];

            if (para_list.Trim() == string.Empty)
            {
                return true;
            }

            string[] div_byComma = para_list.Split(SomeChar.Comma);

            SomeSequence this_seq = null;
            foreach (string para_str in div_byComma)
            {
                SomeParameter parameter = new SomeParameter();
                statement.AddParameter(parameter);
                if (para_str.IndexOf(SomeChar.Equal) != -1)
                {
                    //m_obj=obj1
                    string[] div_byEqual = para_str.Split(SomeChar.Equal);
                    if (div_byEqual.Length != 2)
                    {
                        SomeErrorReporter.AddError(string.Format("cannot parse '{0}'", paren_pair), line_number);
                        return false;
                    }
                    parameter.InvokingObj = div_byEqual[1].Trim();
                    parameter.PassedObj = div_byEqual[1].Trim();

                    SomeExpressionObject left_obj = new SomeExpressionObject();
                    left_obj.Object = div_byEqual[0].Trim();

                    SomeExpressionObject right_obj = new SomeExpressionObject();
                    right_obj.Object = div_byEqual[1].Trim();


                    SomeStatementAssignment assign = new SomeStatementAssignment(
                        left_obj,
                        right_obj,
                        null,
                        null,
                        SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightObj
                        );

                    //a new sub sequence
                    if (this_seq == null)
                    {
                        this_seq = new SomeSequence(statement);
                    }

                    //subSeq.AddInvokingParameter(parameter);
                    this_seq.AddStatement(assign);
                }
                else if (para_str.IndexOf(SomeChar.LBracket) != -1)
                {
                    string[] div_byBrack = para_str.Split(new char[] { SomeChar.LBracket, SomeChar.RBracket });
                    if (div_byBrack.Length != 3)
                    {
                        return false;
                    }
                    parameter.InvokingObj = getIfThisPointerFromParent(parent_seq, div_byBrack[1].Trim());
                    parameter.PassedObj = div_byBrack[0].Trim();
                }
                else
                {
                    //str
                    parameter.InvokingObj = getIfThisPointerFromParent(parent_seq, para_str.Trim());
                    parameter.PassedObj = para_str.Trim();
                }

                parameter.Type = lookupObjTypeNameFromSequence(parent_seq, parameter.InvokingObj);
            }

            if (this_seq != null)
                parent_seq.AddSubSequence(this_seq);

            return true;
        }

        private static string getIfThisPointerFromParent(SomeSequence parent_seq, string obj)
        {
            if (parent_seq != null && parent_seq.AttachedStatement != null)
            {
                string invoking_obj = parent_seq.AttachedStatement.InvokingObject;
                if (!string.IsNullOrEmpty(invoking_obj))
                {
                    if (obj == invoking_obj)
                    {
                        return "this";
                    }
                }
            }

            return obj;
        }

        private static string lookupObjTypeNameFromSequence(SomeSequence parent_seq, string obj_name)
        {
            if (parent_seq == null)
            {
                return obj_name;
            }

            return parent_seq.LookupObjTypeFromSequence(obj_name);
        }

        private static bool tryParseAssignmentStatement(SomeToken begin, SomeToken end, /*SomeSequence seq,*/SomeSequence parent_seq, ref SomeStatement statement)
        {
            SomeToken token = begin;
            if (token.Type == SomeTokenType.Ident)
            {
                string ident1 = token.TokenString;
                token = token.Next;
                switch (token.Type)
                {
                    case SomeTokenType.Ident:
                        {
                            //Ident Ident
                            string ident2 = token.TokenString;
                            SomeExpressionDefination left_def = new SomeExpressionDefination();
                            left_def.DefinationType = ident1;
                            left_def.DefinationObject = ident2;
                            token = token.Next;
                            if (token.Type == SomeTokenType.Equal)
                            {
                                //Ident Ident=
                                token = token.Next;
                                SomeStatement invoking_stat = null;
                                if (tryParseInvokingStatement(token, end, /*seq,*/ parent_seq, ref invoking_stat, SomeTokenType.Equal))
                                {
                                    SomeStatementMethodInvoking invoking = invoking_stat as SomeStatementMethodInvoking;
                                    statement = new SomeStatementAssignment(null, null, left_def, invoking, SomeStatementAssignment.SomeStatementAssignmentType.LeftDefEqualsRightInvoking);
                                    return true;
                                }
                                else
                                {
                                    if (token.Type == SomeTokenType.Ident)
                                    {
                                        string ident3 = token.TokenString;
                                        token = token.Next;
                                        if (token.Type == SomeTokenType.Dot)
                                        {
                                            token = token.Next;
                                            if (token.Type == SomeTokenType.Ident)
                                            {
                                                SomeExpressionObject right_obj = new SomeExpressionObject();
                                                right_obj.Object = ident3;
                                                right_obj.ObjectOfObject = token.TokenString;
                                                statement = new SomeStatementAssignment(null, right_obj, left_def, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftDefEqualsRightObj);
                                                return true;
                                            }
                                        }
                                        else if (token.Type == SomeTokenType.Semi)
                                        {
                                            SomeExpressionObject right_obj = new SomeExpressionObject();
                                            right_obj.Object = ident3;
                                            statement = new SomeStatementAssignment(null, right_obj, left_def, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftDefEqualsRightObj);
                                            return true;
                                        }
                                    }
                                    else if (token.Type == SomeTokenType.Digit || token.Type == SomeTokenType.String)
                                    {
                                        //Ident.Ident = "", Ident.Ident = 1234
                                        SomeExpressionObject right_obj = new SomeExpressionObject();
                                        right_obj.Object = token.TokenString;
                                        statement = new SomeStatementAssignment(null, right_obj, left_def, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftDefEqualsRightObj);
                                        return true;
                                    }
                                }
                            }
                        } break;
                    case SomeTokenType.Dot:
                        {
                            token = token.Next;
                            if (token.Type == SomeTokenType.Ident)
                            {
                                //Ident.Ident
                                string ident2 = token.TokenString;
                                SomeExpressionObject left_obj = new SomeExpressionObject();
                                left_obj.Object = ident1;
                                left_obj.ObjectOfObject = ident2;

                                token = token.Next;
                                if (token.Type == SomeTokenType.Equal)
                                {
                                    //Ident.Ident=
                                    token = token.Next;
                                    SomeStatement invoking_stat = null;
                                    if (tryParseInvokingStatement(token, end, /*seq,*/ parent_seq, ref invoking_stat, SomeTokenType.Equal))
                                    {
                                        SomeStatementMethodInvoking invoking = invoking_stat as SomeStatementMethodInvoking;
                                        statement = new SomeStatementAssignment(left_obj, null, null, invoking, SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightInvoking);
                                        return true;
                                    }
                                    else
                                    {
                                        if (token.Type == SomeTokenType.Ident)
                                        {
                                            //Ident.Ident=Ident
                                            string ident3 = token.TokenString;
                                            token = token.Next;
                                            if (token.Type == SomeTokenType.Dot)
                                            {
                                                token = token.Next;
                                                if (token.Type == SomeTokenType.Ident)
                                                {
                                                    SomeExpressionObject right_obj = new SomeExpressionObject();
                                                    right_obj.Object = ident3;
                                                    right_obj.ObjectOfObject = token.TokenString;
                                                    statement = new SomeStatementAssignment(left_obj, right_obj, null, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightObj);
                                                    return true;
                                                }
                                            }
                                            else if (token.Type == SomeTokenType.Semi)
                                            {
                                                SomeExpressionObject right_obj = new SomeExpressionObject();
                                                right_obj.Object = ident3;
                                                statement = new SomeStatementAssignment(left_obj, right_obj, null, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightObj);
                                                return true;
                                            }
                                        }
                                        else if (token.Type == SomeTokenType.Digit || token.Type == SomeTokenType.String)
                                        {
                                            //Ident.Ident = "", Ident.Ident = 1234
                                            SomeExpressionObject right_obj = new SomeExpressionObject();
                                            right_obj.Object = token.TokenString;
                                            statement = new SomeStatementAssignment(left_obj, right_obj, null, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightObj);
                                            return true;
                                        }
                                    }
                                }
                            }
                        } break;
                    case SomeTokenType.Equal:
                        {
                            SomeExpressionObject left_obj = new SomeExpressionObject();
                            left_obj.Object = ident1;

                            //Ident=
                            token = token.Next;
                            SomeStatement invoking_stat = null;
                            if (tryParseInvokingStatement(token, end, /*seq,*/ parent_seq, ref invoking_stat, SomeTokenType.Equal))
                            {
                                SomeStatementMethodInvoking invoking = invoking_stat as SomeStatementMethodInvoking;
                                statement = new SomeStatementAssignment(left_obj, null, null, invoking, SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightInvoking);
                                return true;
                            }
                            else
                            {
                                if (token.Type == SomeTokenType.Ident)
                                {
                                    //Ident=Ident
                                    string ident3 = token.TokenString;
                                    token = token.Next;
                                    if (token.Type == SomeTokenType.Dot)
                                    {
                                        //Ident=Ident.
                                        token = token.Next;
                                        if (token.Type == SomeTokenType.Ident)
                                        {
                                            SomeExpressionObject right_obj = new SomeExpressionObject();
                                            right_obj.Object = ident3;
                                            right_obj.ObjectOfObject = token.TokenString;
                                            statement = new SomeStatementAssignment(left_obj, right_obj, null, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightObj);
                                            return true;
                                        }
                                    }
                                    else if (token.Type == SomeTokenType.Semi)
                                    {
                                        //Ident=Ident;
                                        SomeExpressionObject right_obj = new SomeExpressionObject();
                                        right_obj.Object = ident3;
                                        statement = new SomeStatementAssignment(left_obj, right_obj, null, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightObj);
                                        return true;
                                    }
                                }
                                else if (token.Type == SomeTokenType.Digit || token.Type == SomeTokenType.String)
                                {
                                    //Ident = "", Ident = 1234
                                    SomeExpressionObject right_obj = new SomeExpressionObject();
                                    right_obj.Object = token.TokenString;
                                    statement = new SomeStatementAssignment(left_obj, right_obj, null, null, SomeStatementAssignment.SomeStatementAssignmentType.LeftObjEqualsRightObj);
                                    return true;
                                }
                            }
                        } break;
                    default:
                        {
                        } break;
                }
            }

            return false;
        }

        private static bool tryParseReturnStatement(SomeToken begin, SomeToken end, /*SomeSequence seq,*/ SomeSequence parent_seq, ref SomeStatement statement)
        {
            SomeToken token = begin;
            switch (token.Type)
            {
                case SomeTokenType.Ident:
                    {
                        string ident1 = token.TokenString;
                        token = token.Next;
                        if (token.Type == SomeTokenType.Dot)
                        {
                            token = token.Next;
                            if (token.Type == SomeTokenType.ParenPair)
                            {
                                //Ident.(..)
                                statement = new SomeStatementReturn(null, null, ident1, SomeStatementReturn.SomeStatementReturnType.ReturnConstrution);
                                return parseStatementInvokingParaList(ident1, ident1, token.TokenString, /*seq,*/ parent_seq, statement, token.LineNumber);
                            }
                            else if (token.Type == SomeTokenType.Ident)
                            {
                                //Ident.Ident
                                string ident2 = token.TokenString;
                                token = token.Next;
                                if (token.Type == SomeTokenType.Semi)
                                {
                                    SomeExpressionObject ret_obj = new SomeExpressionObject();
                                    ret_obj.Object = ident1;
                                    ret_obj.ObjectOfObject = ident2;
                                    statement = new SomeStatementReturn(null, ret_obj, null, SomeStatementReturn.SomeStatementReturnType.ReturnObject);
                                    return true;
                                }
                            }
                        }
                        else if (token.Type == SomeTokenType.Semi)
                        {
                            SomeExpressionObject ret_obj = new SomeExpressionObject();
                            ret_obj.Object = ident1;
                            statement = new SomeStatementReturn(null, ret_obj, null, SomeStatementReturn.SomeStatementReturnType.ReturnObject);
                            return true;
                        }
                    } break;
                case SomeTokenType.Digit:
                    {
                        string const_digit = token.TokenString;
                        statement = new SomeStatementReturn(const_digit, null, null, SomeStatementReturn.SomeStatementReturnType.ReturnConstant);
                        return true;
                    }
                case SomeTokenType.String:
                    {
                        string const_str = token.TokenString;
                        statement = new SomeStatementReturn(const_str, null, null, SomeStatementReturn.SomeStatementReturnType.ReturnConstant);
                        return true;
                    }
                default:
                    break;
            }
            return false;
        }

        //private static bool tryParseSnippetStatement(Token begin, Token end, SomeSequence seq, SomeSequence parent_seq, ref SomeStatement statement)
        //{
        //    return false;
        //}

        //private static bool tryParseCommentStatement(Token begin, Token end, SomeSequence seq, SomeSequence parent_seq, ref SomeStatement statement)
        //{
        //    return true;
        //}

        private static bool tryParseInvokingStatement(SomeToken begin, SomeToken end, SomeSequence parent_seq, ref SomeStatement statement, SomeTokenType previousTokenType)
        {
            SomeToken token = begin;
            if (token.Type == SomeTokenType.Ident)
            {
                //Ident
                string ident1 = token.TokenString;

                if (token == end)
                {
                    if (previousTokenType == SomeTokenType.Equal)
                    {
                        EnterParsingState(EditingStateType.SmsExpectingObjectOrMember, parent_seq);
                        return true;
                    }
                    
                    return false;
                }

                token = token.Next;

                if (token == end)
                {
                    if (token.TokenString.StartsWith(SomeChar.LParen.ToString()))
                    {
                        string typeName = parent_seq.LookupAttachedInvokingType(token.LineNumber);
                        SomeClass callingtype = SomeCompiler.lookupTypeFromName(typeName);
                        ParenStateTransitionSms(token, parent_seq, callingtype);
                        return true;
                    }
                }

                switch (token.Type)
                {
                    case SomeTokenType.Dot:
                        {
                            //Ident.
                            if (token == end)
                            {
                                EnterParsingState(EditingStateType.SmsExpectingMember, lookupObjTypeFromSequence(parent_seq, ident1));
                                return true;
                            }

                            token = token.Next;
                            if (token.Type == SomeTokenType.Ident)
                            {
                                //Ident.Ident
                                string ident2 = token.TokenString;
                                SomeExpressionInvoking exp_invoking = new SomeExpressionInvoking();
                                exp_invoking.Method = ident2;

                                if (ident2 == "main")
                                {
                                    exp_invoking.InvokingObjectType = ident1;
                                    statement = new SomeStatementMethodInvoking(exp_invoking, SomeStatementMethodInvoking.SomeStatementMethodInvokingType.StaticInvoking);
                                    return true;
                                }

                                exp_invoking.InvokingObject = ident1;
                                exp_invoking.InvokingObjectType = lookupObjTypeNameFromSequence(parent_seq, ident1);

                                if (exp_invoking.InvokingObject == exp_invoking.InvokingObjectType)
                                {
                                    //static
                                    statement = new SomeStatementMethodInvoking(exp_invoking, SomeStatementMethodInvoking.SomeStatementMethodInvokingType.StaticInvoking);
                                }
                                else
                                {
                                    //instance
                                    statement = new SomeStatementMethodInvoking(exp_invoking, SomeStatementMethodInvoking.SomeStatementMethodInvokingType.InstanceInvoking);
                                }

                                if (token == end)
                                {
                                    return false;
                                }

                                token = token.Next;
                                if (token.Type == SomeTokenType.ParenPair)
                                {
                                    //Ident.Ident(...)
                                    return parseStatementInvokingParaList(exp_invoking.InvokingObjectType, exp_invoking.Method, token.TokenString, /*seq,*/ parent_seq, statement, token.LineNumber);
                                }
                                else if (token.TokenString.StartsWith(SomeChar.LParen.ToString()))
                                {
                                    if (token == end)
                                    {
                                        SomeClass callingtype = SomeCompiler.lookupTypeFromName(exp_invoking.InvokingObjectType);
                                        ParenStateTransitionSms(token, parent_seq, callingtype);
                                        return true;
                                    }
                                }
                                else if (token.Type == SomeTokenType.Less)
                                {
                                    //Ident.Ident<
                                    if (token == end)
                                    {
                                        EnterParsingState(EditingStateType.SmsExpectingDecendantType, SomeCompiler.lookupTypeFromName(exp_invoking.InvokingObjectType));
                                        return true;
                                    }
                                    
                                    token = token.Next;

                                    if (token.Type == SomeTokenType.Ident)
                                    {
                                        //Ident.Ident<Ident
                                        string ident3 = token.TokenString;
                                        token = token.Next;
                                        exp_invoking.InvokingObjectRealType = ident3;
                                        if (token.Type == SomeTokenType.Greater)
                                        {
                                            //Ident.Ident<Ident>
                                            token = token.Next;
                                            if (token.Type == SomeTokenType.ParenPair)
                                            {
                                                //Ident.Ident<Ident>(...)
                                                return parseStatementInvokingParaList(exp_invoking.InvokingObjectRealType, exp_invoking.Method, token.TokenString, /*seq,*/ parent_seq, statement, token.LineNumber);
                                            }
                                        }
                                    }
                                }

                            }
                        } break;
                    case SomeTokenType.Less:
                        {
                            //Ident<
                            token = token.Next;
                            if (token.Type == SomeTokenType.Ident)
                            {
                                //Ident<Ident
                                string ident2 = token.TokenString;
                                SomeExpressionInvoking exp_invoking = new SomeExpressionInvoking();
                                exp_invoking.InvokingObjectType = ident2;
                                exp_invoking.InvokingObjectRealType = ident2;
                                exp_invoking.Method = ident1;
                                statement = new SomeStatementMethodInvoking(exp_invoking, SomeStatementMethodInvoking.SomeStatementMethodInvokingType.PrivateInvoking);
                                token = token.Next;
                                if (token.Type == SomeTokenType.Greater)
                                {
                                    //Ident<Ident>
                                    token = token.Next;
                                    if (token.Type == SomeTokenType.ParenPair)
                                    {
                                        //Ident<Ident>(...)
                                        return parseStatementInvokingParaList(exp_invoking.InvokingObjectRealType, exp_invoking.Method, token.TokenString, /*seq,*/ parent_seq, statement, token.LineNumber);
                                    }
                                    else if (token.TokenString.StartsWith(SomeChar.LParen.ToString()))
                                    {
                                        if (token == end)
                                        {
                                            SomeClass callingtype = SomeCompiler.lookupTypeFromName(exp_invoking.InvokingObjectType);
                                            ParenStateTransitionSms(token, parent_seq, callingtype);
                                            return true;
                                        }
                                    }
                                }
                            }
                        } break;
                    case SomeTokenType.ParenPair:
                        {
                            //Ident(...)
                            SomeExpressionInvoking exp_inv = new SomeExpressionInvoking();
                            if (parent_seq == null)
                            {
                                //Error
                                throw new SomeCompilerException("there should be a parent sequence", token.LineNumber);
                            }
                            else
                            {
                                exp_inv.InvokingObjectType = parent_seq.LookupAttachedInvokingType(token.LineNumber);
                            }
                            exp_inv.Method = ident1;
                            statement = new SomeStatementMethodInvoking(exp_inv, SomeStatementMethodInvoking.SomeStatementMethodInvokingType.PrivateInvoking);
                            return parseStatementInvokingParaList(exp_inv.InvokingObjectType, exp_inv.Method, token.TokenString, /*seq,*/ parent_seq, statement, token.LineNumber);
                        }
                    default:
                        {
                            return false;
                        }
                }
            }

            return false;
        }

    }
}
