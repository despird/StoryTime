using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SOME.SomeLanguageCore.CodeDom;
using SOME.SomeLanguageCore.Tokens;
using System.Text.RegularExpressions;
using System.CodeDom;
using System.Linq;
using SOME.SomeLanguageCore.Parser;

namespace SOME.SomeLanguageCore
{
    public static class SomeCompiler
    {
        [ThreadStatic]
        private static SomeOneLineReader _reader;

        [ThreadStatic]
        public static string CodeName;

        [ThreadStatic]
        private static SomeTokenList _tokenList;

        public static SomeTokenList TokenList
        {
            get
            {
                if (_tokenList == null)
                {
                    _tokenList = new SomeTokenList();
                }
                return _tokenList;
            }
            set { SomeCompiler._tokenList = value; }
        }

        [ThreadStatic]
        private static SomeDom _codeDom;

        public static SomeDom CodeDom
        {
            get
            {
                if (_codeDom == null)
                    _codeDom = new SomeDom();

                return _codeDom;
            }
        }

        public static void ResetCodeDom()
        {
            _codeDom = new SomeDom();
        }

        public static void Compile(string[] files)
        {
            ResetCodeDom();
            SomeErrorReporter.Clear();

            foreach (string file in files)
            {
                string ext = Path.GetExtension(file);
                if (ext == ".smr" || ext == ".som")
                {
                    CodeName = Path.GetFileNameWithoutExtension(file);
                }

                SomeOneLineReader reader = new SomeOneLineReader(file, Encoding.Default);
                SomeErrorReporter.CurrentFile = file;
                doCompile(reader);
                reader.Close();
            }
        }

        private static void doCompile(SomeOneLineReader reader)
        {
            _reader = reader;
            lex();
            //testOutput();

            try
            {
                parse();
            }
            catch (SomeCompilerException ex)
            {
                SomeErrorReporter.AddError(ex.Message, ex.LineNo);
            }
            catch (Exception ex)
            {
                SomeErrorReporter.AddError("unknown error", -1);
            }
        }

        private static void testOutput()
        {
            SomeErrorReporter.Clear();
            for (SomeToken token = TokenList.First; token != null; token = token.NextToken)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(token.LineNumber.ToString()).Append(" ").Append(token.TokenString).Append(" ").Append(token.Type.ToString());
                SomeErrorReporter.AddError(sb.ToString());
                sb = null;
            }
        }


        private static void lex()
        {
            TokenList.Clear();
            for (string sline = _reader.ReadLine(); sline != null; sline = _reader.ReadLine())
            {
                lexline(TokenList, _reader.Line, sline);
                TokenList.Add(new SomeToken(_reader.Line, 0, 0, Environment.NewLine, SomeTokenType.NewLine));
            }
        }

        public static void lex(SomeTokenList tokenLIst, string content)
        {
            using (StringReader reader = new StringReader(content))
            {
                string line = null;
                int lineNo = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    lexline(tokenLIst, lineNo, line);
                    lineNo++;
                }
            }

        }

        public static void lexline(SomeTokenList tokenList, int line, string sline)
        {
            if (sline.Trim() == string.Empty)
            {
                tokenList.Add(new SomeToken(line, 0, sline.Length, "", SomeTokenType.EmptyLine));
                return;
            }

            for (int i = 0; i < sline.Length; )
            {
                char c = sline[i];
                int start = i; //save column start
                //identifier
                if (SomeChar.IsIdenCharStart(c))
                {
                    StringBuilder sb = new StringBuilder(c.ToString());
                    while ((++i) < sline.Length && SomeChar.IsIdentChar(sline[i]))
                    {
                        sb.Append(sline[i].ToString());
                    }
                    //collection 
                    if (i + 1 < sline.Length && sline[i] == SomeChar.LBracket && sline[i + 1] == SomeChar.RBracket)
                    {
                        i += 2;
                        sb.Append("[]");
                    }
                    tokenList.Add(new SomeToken(line, start, sb.Length, sb.ToString(), SomeTokenType.Ident));
                    sb = null;
                }
                //digit
                else if (SomeChar.IsDigit(c))
                {
                    StringBuilder sb = new StringBuilder(c.ToString());
                    while ((++i) < sline.Length && SomeChar.IsDigit(sline[i]))
                    {
                        sb.Append(sline[i].ToString());
                    }
                    if (i < sline.Length && sline[i] == SomeChar.Dot)
                    {
                        while ((++i) < sline.Length && SomeChar.IsDigit(sline[i]))
                        {
                            sb.Append(sline[i].ToString());
                        }
                    }
                    tokenList.Add(new SomeToken(line, start, sb.Length, sb.ToString(), SomeTokenType.Digit));
                    sb = null;
                }
                else
                {
                    switch (c)
                    {
                        //begin with dot
                        case SomeChar.Dot:
                            {
                                //if it's a digit
                                ++i;
                                if (sline.Length > i && SomeChar.IsDigit(sline[i]))
                                {
                                    StringBuilder sb = new StringBuilder(c.ToString());
                                    while (i < sline.Length && SomeChar.IsDigit(sline[i]))
                                    {
                                        sb.Append(sline[i].ToString());
                                        ++i;
                                    }
                                    tokenList.Add(new SomeToken(line, start, sb.Length, sb.ToString(), SomeTokenType.Digit));
                                    ++i;
                                }
                                else // just a point
                                {
                                    tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Dot));
                                }
                            } break;
                        //blank space
                        case SomeChar.Blank:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, " ", SomeTokenType.Blank));
                                ++i;

                                //while ((++i) < sline.Length && (sline[i] == SomeChar.Blank || sline[i] == SomeChar.Tab))
                                //{
                                //    //do nothing
                                //}
                            } break;
                        case SomeChar.Tab:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, "\t", SomeTokenType.Blank));
                                ++i;
                                //while ((++i) < sline.Length && (sline[i] == SomeChar.Blank || sline[i] == SomeChar.Tab))
                                //{
                                //    //do nothing
                                //}
                            } break;
                        //string
                        case SomeChar.Quote:
                            {
                                StringBuilder sb = new StringBuilder(c.ToString());
                                while ((++i) < sline.Length && sline[i] != SomeChar.Quote)
                                {
                                    sb.Append(sline[i].ToString());
                                }
                                if (i == sline.Length)
                                {
                                    SomeErrorReporter.AddError("\" expected.", line);
                                    tokenList.Add(new SomeToken(line, start, sb.Length, sb.ToString(), SomeTokenType.Unknown));
                                    break;
                                }
                                sb.Append(SomeChar.Quote.ToString());

                                tokenList.Add(new SomeToken(line, start, sb.Length, sb.ToString(), SomeTokenType.String));
                                sb = null;
                                ++i;
                            } break;
                        case SomeChar.Equal:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Equal));
                                ++i;
                            } break;
                        case SomeChar.LCurly:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.LCurly));
                                ++i;
                            } break;
                        case SomeChar.RCurly:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.RCurly));
                                ++i;
                            } break;
                        case SomeChar.LBracket:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.LBracket));
                                ++i;
                            } break;
                        case SomeChar.RBracket:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.RBracket));
                                ++i;
                            } break;
                        case SomeChar.LParen:
                            {
                                StringBuilder sb = new StringBuilder(c.ToString());
                                while ((++i) < sline.Length && sline[i] != SomeChar.RParen)
                                {
                                    sb.Append(sline[i].ToString());
                                }
                                if (i == sline.Length)
                                {
                                    //Error: no right paren
                                    SomeErrorReporter.AddError(") expected.", line);
                                    tokenList.Add(new SomeToken(line, start, sb.Length, sb.ToString(), SomeTokenType.Unknown));
                                    break;
                                }
                                sb.Append(SomeChar.RParen.ToString());

                                tokenList.Add(new SomeToken(line, start, sb.Length, sb.ToString(), SomeTokenType.ParenPair));
                                sb = null;
                                ++i;
                            } break;
                        case SomeChar.Colon:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Colon));
                                ++i;
                            } break;
                        case SomeChar.Semi:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Semi));
                                ++i;
                            } break;
                        case SomeChar.Less:
                            {
                                if ((++i) < sline.Length && sline[i] == SomeChar.Percent)
                                {
                                    tokenList.Add(new SomeToken(line, start, 2, SomeTokenTypeString.SnippetBegin, SomeTokenType.SnipBegin));
                                    ++i;
                                }
                                else
                                {
                                    tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Less));
                                }
                            } break;
                        case SomeChar.Greater:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Greater));
                                ++i;
                            } break;
                        case SomeChar.Percent:
                            {
                                if ((++i) < sline.Length && sline[i] == SomeChar.Greater)
                                {
                                    tokenList.Add(new SomeToken(line, start, 2, SomeTokenTypeString.SnippetEnd, SomeTokenType.SnipEnd));
                                    ++i;
                                }
                                else
                                {
                                    tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Percent));
                                }
                            } break;
                        case SomeChar.Comma:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Comma));
                                ++i;
                            } break;
                        case SomeChar.Tilde:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Tilde));
                                ++i;
                            } break;
                        case SomeChar.Minus:
                            {
                                if ((++i) < sline.Length && sline[i] == SomeChar.Greater)
                                {
                                    tokenList.Add(new SomeToken(line, start, 2, "->", SomeTokenType.Referrence));
                                    ++i;
                                }
                                else
                                {
                                    tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Unknown));
                                }
                            } break;
                        case SomeChar.Slash:
                            {
                                if ((++i) < sline.Length && sline[i] == SomeChar.Slash)
                                {
                                    tokenList.Add(new SomeToken(line, start, sline.Substring(i - 1).Length, sline.Substring(i - 1), SomeTokenType.SingleLineComment));
                                    return;
                                }
                                else
                                {
                                    tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Unknown));
                                }
                            } break;
                        case SomeChar.Return:
                            {
                                if ((++i) < sline.Length && sline[i] == SomeChar.NewLine)
                                {
                                    tokenList.Add(new SomeToken(line, start, 2, "\r\n", SomeTokenType.NewLine));
                                    ++i;
                                }
                                else
                                {
                                    tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.NewLine));
                                }
                            } break;
                        default:
                            {
                                tokenList.Add(new SomeToken(line, start, 1, c.ToString(), SomeTokenType.Unknown));
                                ++i;
                            } break;
                    }
                }
            }
        }


        #region Used for Editor syntax colouring/Auto completion
        public static List<SomeClass> Classes
        {
            get { return CodeDom.Classes; }
        }

        public static IEnumerable<string> ClassNames
        {
            get
            {
                return Classes.Select(cls => cls.ClassName);
            }
        }

        public static void AddClass(SomeClass cls)
        {
            CodeDom.AddClass(cls);
        }

        public static void BuildRelations(bool rebuild)
        {
            CodeDom.BuildRelations(rebuild);
        }

        public static void ParseTypes(string text, string codeName)
        {
            SomeTokenList tokenList = new SomeTokenList();
            try
            {
                lex(tokenList, text);
                parseTypes(tokenList, text, codeName);
            }
            catch { }

        }

        private static void parseTypes(SomeTokenList tokenList, string text, string codeName)
        {
            TokenList = tokenList;
            CodeName = codeName;

            for (SomeToken token = TokenList.First; token != null; token = token.Next)
            {
                if (token.Type == SomeTokenType.Ident)
                {
                    SomeToken tokenNext = token.Next;
                    if (tokenNext == null)  //only an type declaration
                    {
                        AddClass(SomeParser.ParseType(token, CodeName));
                        break;
                    }

                    //type
                    if (tokenNext.Type != SomeTokenType.Dot)
                    {
                        SomeToken begin = token;
                        for (; token != null && token.Type != SomeTokenType.EmptyLine; token = token.Next) { }
                        SomeToken end = token;

                        AddClass(SomeParser.ParseType(begin, end, CodeName));
                        if (token == null)
                        {
                            break;
                        }
                    }
                    else //sequence encountered, this must be a .sms
                    {
                        return;
                    }
                }
            }

            BuildRelations(true);
        }

        public static void ParseSequence(string text)
        {
            SomeTokenList tokenList = new SomeTokenList();
            try
            {
                lex(tokenList, text);
            }
            catch { }

            TokenList = tokenList;

            CodeDom.ClearSequences();
            SomeParser.CombineSnippet(TokenList.First, TokenList.Current);

            SomeParser.ParseSequence2(ref TokenList.First, TokenList.Current, null, CodeDom);
            //CodeDom.Parse();
        }


        public static void RemoveByNameSpace(string codeName)
        {
            string nameSpace = codeName[0].ToString().ToUpper() + codeName.Substring(1);
            Classes.RemoveAll(c => c.NameSpace == nameSpace);
        }

        public const string SOME_EXT_SMR = ".smr";
        public const string SOME_EXT_SMS = ".sms";
        #endregion

        private static void parse()
        {
            for (SomeToken token = TokenList.First; token != null; token = token.Next)
            {
                if (token.Type == SomeTokenType.Ident)
                {
                    SomeToken tokenNext = token.Next;
                    if (tokenNext == null)  //only an type declaration
                    {
                        CodeDom.AddClass(SomeParser.ParseType(token, CodeName));
                        break;
                    }

                    //type
                    if (tokenNext.Type != SomeTokenType.Dot)
                    {
                        SomeToken begin = token;
                        for (; token != null && token.Type != SomeTokenType.EmptyLine; token = token.Next) { }
                        SomeToken end = token;

                        CodeDom.AddClass(SomeParser.ParseType(begin, end, CodeName));
                    }
                    //sequence
                    else
                    {
                        SomeToken begin = token;
                        SomeToken end = null;

                        for (; token != null && token.Type != SomeTokenType.LCurly; token = token.Next) { }

                        if (token.Type == SomeTokenType.LCurly)
                        {
                            int pair = 1;
                            for (token = token.Next; token != null && pair != 0; token = token.Next)
                            {
                                if (token.Type == SomeTokenType.LCurly)
                                {
                                    ++pair;
                                }
                                if (token.Type == SomeTokenType.RCurly)
                                {
                                    --pair;
                                }
                            }
                            if (pair == 0)
                            {
                                end = token;
                            }
                            else
                            {
                                //error : curly brackets don't match
                                break;
                            }
                        }
                        else
                        {
                            //Error: empty sequence definition
                            SomeErrorReporter.AddError("Empty sequence definition.", token.LineNumber);
                            break;
                        }

                        SomeParser.CombineSnippet(begin, end);
                        CodeDom.AddSequence(SomeParser.ParseSequence(begin, end, null));
                    }

                    if (token == null)  //end of tokens
                    {
                        break;
                    }
                }
                else if (token.Type != SomeTokenType.EmptyLine)
                {
                    SomeErrorReporter.AddError("Error: neither type declaration nor sequence definition!", token.LineNumber);
                }
            }

            CodeDom.Parse();
        }

        public static SomeClass lookupTypeFromName(string typeName)
        {
            return Classes.Find(cls => cls.ClassName == typeName);
        }

        private static void check()
        {
            CodeDom.Check();
        }

        public static bool checkIsDesendent(string type_descend, string type_ancestor)
        {
            foreach (SomeClass cls in CodeDom.Classes)
            {
                if (cls.ClassName == type_descend)
                {
                    return cls.CheckIsDesendent(type_ancestor);
                }
            }
            return false;
        }

        public static bool IsTypeName(string ident1)
        {
            foreach (SomeClass cls in CodeDom.Classes)
            {
                if (ident1 == cls.ClassName)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
