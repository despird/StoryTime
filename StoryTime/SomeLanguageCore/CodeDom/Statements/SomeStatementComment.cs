using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeStatementComment : SomeStatement
    {
        public string Comment = "";

        public SomeStatementComment(string comment)
            : base(SomeStatementType.Comment)
        {
            Comment = comment;
        }

        public override System.CodeDom.CodeStatement GetMSCodeDomStatement()
        {
            return new CodeCommentStatement(this.Comment.Replace(SomeTokenTypeString.SingleLineComment, string.Empty));
        }

    }

}
