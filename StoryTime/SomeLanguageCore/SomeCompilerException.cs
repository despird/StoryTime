using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageCore
{
    public class SomeCompilerException: Exception
    {
        public SomeCompilerException(string message, int line): base(message)
        {
            lineNo = line;
        }

        private int lineNo;

        public int LineNo
        {
            get { return lineNo; }
            set { lineNo = value; }
        }
    }
}
