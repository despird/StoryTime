using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Runtime;

namespace SOME.SomeLanguageCore
{
    public static class SomeErrorReporter
    {
        [ThreadStatic]
        private static List<SomeErrorReport> _reports = new List<SomeErrorReport>();
        public static List<SomeErrorReport> Reports
        {
            get { return _reports; }
            set { _reports = value; }
        }

        [ThreadStatic]
        private static string currentFile = string.Empty;
        public static string CurrentFile
        {
            get { return SomeErrorReporter.currentFile; }
            set { SomeErrorReporter.currentFile = value; }
        }


        [ThreadStatic]
        private static int _currentLine = 0;

        public static int CurrentLine
        {
            get
            {
                return _currentLine;
            }
        }

        public static void NextLine()
        {
            _currentLine++;
        }

        public static void Clear()
        {
            clearError();
        }

        public static void clearError()
        {
            if (_reports != null)
            {
                _reports.Clear();
            }
        }

        public static bool HasErrors
        {
            get
            {
                return _reports.Any(r => r.Level == SomeErrorLevel.Error);
            }
        }

        public static List<SomeErrorReport> GetReports()
        {
            return _reports;
        }

        public static void AddError(string error)
        {
            AddError(error, currentFile, _currentLine);
        }

        public static void AddError(string error, int lineNo)
        {
            AddError(error, currentFile, lineNo);
        }

        public static void AddError(string error, string file)
        {
            AddError(error, file, _currentLine);
        }

        public static void AddError(string error, string file, int lineNo)
        {
            _reports.Add(new SomeErrorReport()
            {
                File = file,
                Level = SomeErrorLevel.Error,
                LineNumber = lineNo,
                Message = error
            });
        }

        public static void AddWarning(string waring, string file)
        {
            _reports.Add(new SomeErrorReport()
            {
                File = file,
                Level = SomeErrorLevel.Warning ,
                LineNumber = _currentLine,
                Message = waring 
            });
        }
    }
}
