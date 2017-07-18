using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageCore.Tokens
{
    public enum SomeTokenType : byte
    {
        None,
        Ident,      //identifier, begin with '_' or letter 
        Digit,      //digit
        String,
        Dot,        //.
        Equal,       //=
        Blank,
        LCurly,
        RCurly,
        ParenPair,
        LBracket,
        RBracket,
        CollectDef,
        Colon,
        Semi,
        Less,
        Greater,
        SnipBegin,      // "<%"
        SnipEnd,        // "%>"
        Snippet,
        Percent,
        Referrence,     // ->
        Comma,
        SingleLineComment,  //Only single comment is supported currently
        //CommentStart,
        //CommentEnd,
        Tilde,      //~
        EmptyLine,
        NewLine,
        Unknown
    }
}
