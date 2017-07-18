using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using SOME.SomeLanguageCore.CodeDom;
using SOME.SomeLanguageCore.Tokens;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;

namespace SOME.SomeLanguageGenerator
{
    public enum SomeLanguage
    {
        CSharp,
        VBNet,
    }

    public abstract class SomeGenerator
    {
        //public abstract bool Generate(SomeDom dom, string path);

        public abstract string GetSource();

        public abstract void GenerateSource(string srcPath);

        public abstract string GetClass(SomeClass cls);

        public abstract string GetField(SomeField field);

        public abstract string GetMethod(SomeMethod method);

        public abstract string GetPropery(SomeProperty property);

        public abstract string GetSourceName();

        public abstract string GetSourceNameForTextEditorControl();

        public abstract string GetSourceFileExtension();

        public abstract string[] GetLangugeTemplate();

        public abstract IEnumerable<string> GenerateProjectSource(string projDir, string projName);

        public abstract IEnumerable<string> GetGenerateProjectNames();
        protected SomeDom m_codeDom;

        public static SomeGenerator GetConcreteGenerator(SomeLanguage lang, SomeDom dom)
        {
            if (lang == SomeLanguage.CSharp)
            {
                return new CSharpGenerator(dom);
            }
            else if (lang == SomeLanguage.VBNet)
            {
                return new VBGenerator(dom);
            }
            throw new NotImplementedException();
        }

        //public abstract string GenerateMethod();
        //public abstract string GenerateField();
        //public abstract string GenerateProperty();

        public string Indent(int n)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; i++)
            {
                sb.Append("    ");
            }
            return sb.ToString();
        }

        public SomeGenerator(SomeDom codeDom)
        {
            this.m_codeDom = codeDom;
        }
    }
}
