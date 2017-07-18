using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SOME.SomeLanguageCore.CodeDom;
using SOME.SomeLanguageCore.Tokens;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

using Microsoft.VisualBasic;

namespace SOME.SomeLanguageGenerator
{
    public class VBGenerator : MSCodeGenerator
    {
        public override CodeDomProvider GetCodeDomProvider()
        {
            return new VBCodeProvider();
        }

        public VBGenerator(SomeDom codeDom)
            : base(codeDom)
        {
        }

        public override string GetClass(SomeClass cls)
        {
            throw new NotImplementedException();
        }

        public override string GetField(SomeField field)
        {
            throw new NotImplementedException();
        }

        public override string GetMethod(SomeMethod method)
        {
            throw new NotImplementedException();
        }

        public override string GetPropery(SomeProperty property)
        {
            throw new NotImplementedException();
        }

        public override string GetSourceName()
        {
            return "VB.net";
        }

        public override string GetSourceNameForTextEditorControl()
        {
            return "VBNET";
        }

        public override string GetSourceFileExtension()
        {
            return ".vb";
        }

        public override string[] GetLangugeTemplate()
        {
            return new string[] { "ClassLibrary.zip", "VisualBasic" };
        }

    }
}
