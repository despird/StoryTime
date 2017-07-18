using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;

namespace SOME.SomeLanguageService
{
    class SomeSource : Source
    {
        public SomeSource(LanguageService service,
                        IVsTextLines textLines,
                        Colorizer colorizer)
            : base(service, textLines, colorizer)
        {
        }
    }
}
