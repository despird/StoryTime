using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;

namespace SOME.SomeLanguageService
{
    class SomeScanner: IScanner
    {
        #region IScanner Members

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            return true;
        }

        public void SetSource(string source, int offset)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
