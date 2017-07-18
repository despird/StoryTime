using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SOME.SomeLanguageCore
{
    public class SomeTextReader : TextReader
    {
        public override string ReadLine()
        {
            SomeErrorReporter.NextLine();
            return base.ReadLine();
        }
    }
}
