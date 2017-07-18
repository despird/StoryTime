﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Compiler;
using IronPython.Runtime;
using IronPython.Hosting;
using Microsoft.VisualStudio.Text;

namespace SOME.SomeLanguageService.EditorExtensions
{
    /// <summary>
    /// Provides errors that should be added to the error list
    /// </summary>
    internal class SomErrorListProvider
    {
        /// <summary>
        /// Gets the errors of the text buffer
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <returns></returns>
        internal IList<ValidationError> GetErrors(ITextBuffer textBuffer)
        {
            var sink = new PyErrorListCompilerSink(textBuffer);
            var modules = new Modules();
            modules.AnalyzeModule(sink, textBuffer.GetFileName(), textBuffer.CurrentSnapshot.GetText());

            return sink.Errors.ToList();
        }
    }
}
