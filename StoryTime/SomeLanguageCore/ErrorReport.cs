using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageCore
{
    public enum SomeErrorLevel
    {
        None,
        Warning,
        Error
    }

    public class SomeErrorReport
    {
        private string file;
        public string File
        {
            get { return file; }
            set { file = value; }
        }

        private int _errorCode;    //TODO
        public int ErrorCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        private int lineNumber;
        public int LineNumber
        {
            get { return lineNumber; }
            set { lineNumber = value; }
        }

        private int columnNumber; //TODO
        public int ColumnNumber
        {
            get { return columnNumber; }
            set { columnNumber = value; }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private SomeErrorLevel level = SomeErrorLevel.None;
        public SomeErrorLevel Level
        {
            get { return level; }
            set { level = value; }
        }
    }   
}
