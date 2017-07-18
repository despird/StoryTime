using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;
using SOME.SomeLanguageCore;

namespace SOME.SomeLanguageProject.CompilerTask
{
    public class SomeCompilerTask: Task
    {
        private string[] sourceFiles;
        /// <summary>
        /// List of Python source files that should be compiled into the assembly
        /// </summary>
        [Required()]
        public string[] SourceFiles
        {
            get { return sourceFiles; }
            set { sourceFiles = value; }
        }

        private string outputAssembly;
        /// <summary>
        /// Output Assembly (including extension)
        /// </summary>
        [Required()]
        public string OutputAssembly
        {
            get { return outputAssembly; }
            set { outputAssembly = value; }
        }


        private string projectPath = null;
        /// <summary>
        /// This should be set to $(MSBuildProjectDirectory)
        /// </summary>
        public string ProjectPath
        {
            get { return projectPath; }
            set { projectPath = value; }
        }

        public SomeCompilerTask()
        {

        }

        public override bool Execute()
        {
            Log.LogMessage(MessageImportance.Normal, "Some Compiling Start...");
            string outputPath = Path.Combine(ProjectPath, OutputAssembly);
            File.Create(outputPath);
            SomeCompiler.Compile(sourceFiles);
            if (SomeErrorReporter.HasErrors)
            {
                SomeErrorReporter.GetReports().ForEach(report => 
                {
                    Log.LogError(
                        string.Empty,
                        report.ErrorCode.ToString(),
                        string.Empty,
                        report.File,
                        report.LineNumber,
                        report.ColumnNumber,
                        report.LineNumber,
                        report.ColumnNumber,
                        report.Message);
                });

                return false;
            }
            return true;
        }
    }
}
