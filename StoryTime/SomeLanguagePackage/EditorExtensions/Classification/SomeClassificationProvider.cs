using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Editor;

namespace SOME.SomeLanguageService.EditorExtensions.Classification
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(SomeContentTypeDefinition.ContentType1)]
    [ContentType(SomeContentTypeDefinition.ContentType2)]
    internal class SomeClassificationProvider : IClassifierProvider
    {
        [Import]
        IClassificationTypeRegistryService classificationRegistryService { get; set; }

        IClassifier IClassifierProvider.GetClassifier(ITextBuffer textBuffer)
        {
            // Creates the python classifier
            return new SomeClassifier(textBuffer, classificationRegistryService);
        }
    }
}