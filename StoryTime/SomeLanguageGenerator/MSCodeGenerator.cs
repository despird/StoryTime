using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.CodeDom;
using System.CodeDom.Compiler;

using SOME.SomeLanguageCore.CodeDom;

namespace SOME.SomeLanguageGenerator
{
    public abstract class MSCodeGenerator: SomeGenerator
    {
        public MSCodeGenerator(SomeDom codeDom)
            : base(codeDom)
        {
        }

        public override string GetSource()
        {
            CodeCompileUnit unit = MSCodeDomConverter.ConvertSomeTreeToCodeDom(this.m_codeDom);

            string fileName = System.Guid.NewGuid().ToString() + ".tmp";

            GenerateMSCode(unit, fileName);

            string source;

            using (StreamReader sr = new StreamReader(fileName))
            {
                source = sr.ReadToEnd();
                sr.Close();
            }

            File.Delete(fileName);
            return source;
        }

        public override void GenerateSource(string srcPath)
        {
            CodeCompileUnit unit = MSCodeDomConverter.ConvertSomeTreeToCodeDom(this.m_codeDom);
            GenerateMSCode(unit, srcPath);
        }

        private List<CodeCompileUnit> _units = null;

        public override IEnumerable<string> GetGenerateProjectNames()
        {
            _units = MSCodeDomConverter.ConvertSomeTreeToCodeDomMultiFile(this.m_codeDom);
            var result = from unit in _units select unit.Namespaces[0].Name;
            return result.Distinct();
        }

        public override IEnumerable<string> GenerateProjectSource(string projDir, string projName)
        {
            var units = from unit in _units
                         where unit.Namespaces[0].Name == projName
                         select unit;

            var srcFiles = new List<string>();
            foreach (CodeCompileUnit unit in units)
            {
                string srcFile = Path.Combine(projDir, unit.Namespaces[0].Types[0].Name + GetSourceFileExtension());
                GenerateMSCode(unit, srcFile);
                srcFiles.Add(srcFile);
            }

            return srcFiles;
        }

        public void GenerateMSCode(CodeCompileUnit compileunit, string sourceFile)
        {
            // Generate the code with the C# code provider.
            CodeDomProvider provider = GetCodeDomProvider();

            MemoryStream stream = new MemoryStream();

            StreamWriter sw = new StreamWriter(stream);

            // Create a TextWriter to a StreamWriter to the output file.
            IndentedTextWriter tw = new IndentedTextWriter(new StreamWriter(sourceFile, false), "    ");

            // Generate source code using the code provider.
            CodeGeneratorOptions codeGen = new CodeGeneratorOptions();

            codeGen.BracingStyle = "C";

            provider.GenerateCodeFromCompileUnit(compileunit, tw, codeGen);

            // Close the output file.
            tw.Close();
        }

        public abstract CodeDomProvider GetCodeDomProvider();
    }
}
