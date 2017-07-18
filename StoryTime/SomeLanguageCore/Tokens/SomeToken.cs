using System;
using System.Collections.Generic;
using System.Text;

namespace SOME.SomeLanguageCore.Tokens
{
    public class SomeToken
    {
        public int LineNumber ;
        public int ColumnStart;
        public int ColumnEnd;
        public string TokenString;
        public SomeTokenType Type;
   
        //private SomeToken _previousToken = null;
        //public SomeToken PreviousToken
        //{
        //    get
        //    {
        //        return _previousToken;
        //    }
        //    set
        //    {
        //        _previousToken = value;
        //    }
        //}

        private SomeToken _next_token = null;
        public SomeToken NextToken
        {
            get
            {
                return _next_token;
            }
            set
            {
                _next_token = value;
                //_next_token.PreviousToken = this;
            }
        }

        //Next token that is not an empty space
        public SomeToken Next
        {
            get
            {
                SomeToken next = _next_token;
                while (next != null &&
                    (next.Type == SomeTokenType.Blank || next.Type == SomeTokenType.NewLine || checkNextTailComment(this, next) /*|| next.Type == TokenType.EmptyLine*/)
                    )
                {
                    next = next.NextToken;
                }
                return next;
            }
        }

        //a comment at the end of the line will be ignored
        private bool checkNextTailComment(SomeToken token, SomeToken next)
        {
            if(next.Type == SomeTokenType.SingleLineComment && token.LineNumber == next.LineNumber)
            {
                return true;
            }

            return false;
        }

        public SomeToken(int lineNumber, int columnStart, int length, string tokenString ,SomeTokenType type)
        {
            LineNumber = lineNumber;
            TokenString = tokenString;
            Type = type;
            ColumnStart = columnStart;
            ColumnEnd = columnStart + length;
        }

        public override string ToString()
        {
            return string.Format("({0},{1}):\"{2}\"", LineNumber, ColumnStart, TokenString);
        }

        public string StringToEnd
        {
            get
            {
                StringBuilder sb = new StringBuilder(this.TokenString);
                SomeToken next = this.NextToken;
                while (next != null)
                {
                    sb.Append(next.TokenString);
                    next = next.NextToken;
                }

                return sb.ToString();
            }
        }
    }


}
