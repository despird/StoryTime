using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using System.Runtime.InteropServices;
using SOME.SomeLanguageCore;

namespace SOME.SomeLanguageService
{
    [ComVisible(true)]
    [Guid(GuidList.guidSomeLangServiceClassString)]
    public class SomeLanguageService // : LanguageService
    {
        //private LanguagePreferences _references;
        //private IScanner _scanner;

        public const string LANGUAGE_NAME = "SOME";
        public readonly static string LANGUAGE_EXT1 = SomeCompiler.SOME_EXT_SMR;
        public readonly static string LANGUAGE_EXT2 = SomeCompiler.SOME_EXT_SMS;

        //public override string GetFormatFilterList()
        //{
        //    return "Some Role Files (*.smr)\n*.smr\n*Some Story Files (*.sms)\n*.sms";
        //}

        //public override ViewFilter CreateViewFilter(CodeWindowManager mgr, IVsTextView newView)
        //{
        //    return new SomeViewFilter(mgr, newView);
        //}

        //public override LanguagePreferences GetLanguagePreferences()
        //{
        //    if (_references == null)
        //    {
        //        _references = new LanguagePreferences(this.Site,
        //                                                typeof(SomeLanguageService).GUID,
        //                                                this.Name);
        //        _references.Init();
        //        _references.IndentStyle = IndentingStyle.Smart;
        //    }

        //    return this._references;
        //}

        //public override IScanner GetScanner(IVsTextLines buffer)
        //{
        //    if (_scanner == null)
        //        _scanner = new SomeScanner();
        //    return _scanner;
        //}

        //public override string Name
        //{
        //    get { return LANGUAGE_NAME; }
        //}

        //public override AuthoringScope ParseSource(ParseRequest req)
        //{
        //    return new SomeAuthoringScope();
        //}
    }
}
