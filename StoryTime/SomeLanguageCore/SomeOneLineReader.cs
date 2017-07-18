using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SOME.SomeLanguageCore
{
    public class SomeOneLineReader: StreamReader
    {
        int _currentLine = 0;

        public SomeOneLineReader(string path,Encoding encoding)
            : base(path,encoding)
        {
        }

        public int Line
        {
            get
            {
                return _currentLine;
            }
        }

        public override string ReadLine()
        {
            _currentLine++;
            return base.ReadLine();
        }
    }

}
