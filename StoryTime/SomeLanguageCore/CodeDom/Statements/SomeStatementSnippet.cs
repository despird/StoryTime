using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SOME.SomeLanguageCore.Tokens;
using System.CodeDom;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeStatementSnippet : SomeStatement
    {
        public string Snippet = "";
        public SomeStatementSnippet(string snippet)
            : base(SomeStatementType.Snippet)
        {
            Snippet = snippet.Replace(SomeTokenTypeString.SnippetBegin, string.Empty).Replace(SomeTokenTypeString.SnippetEnd, string.Empty);
        }

        public override System.CodeDom.CodeStatement GetMSCodeDomStatement()
        {
            return new CodeSnippetStatement(this.Snippet);
        }

    }
}
