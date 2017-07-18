using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using System.Windows.Media;

namespace SOME.SomeLanguageService.EditorExtensions.Classification
{
    /// <summary>
    /// Classification definitions
    /// </summary>
    internal class SomeClassificationDefinitions
    {
        //TODO: using GUID for the all NameAttribute to avoid conflict

        [Name(SomeClassificationTypes.Comment), Export]
        internal ClassificationTypeDefinition CommentClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.Comment)]
        [Name("SomeCommentFormatDefinition")]
        [Order]
        internal sealed class CommentClassificationFormat : ClassificationFormatDefinition
        {
            internal CommentClassificationFormat()
            {
                ForegroundColor = Colors.Green;
                this.DisplayName = "Some Comment";
            }
        }

        [Name(SomeClassificationTypes.Snippet), Export]
        internal ClassificationTypeDefinition SnippetClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.Snippet)]
        [Name("SomeSnippetFormatDefinition")]
        [Order]
        internal sealed class SnippetClassificationFormat : ClassificationFormatDefinition
        {
            internal SnippetClassificationFormat()
            {
                ForegroundColor = Colors.Gray;
                this.DisplayName = "Some Snippet";
            }
        }

        [Name(SomeClassificationTypes.Delimiter), Export]
        internal ClassificationTypeDefinition DelimiterClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.Delimiter)]
        [Name("SomeDelimiterFormatDefinition")]
        [Order]
        internal sealed class DelimiterClassificationFormat : ClassificationFormatDefinition
        {
            public DelimiterClassificationFormat()
            {
                this.DisplayName = "Some Delimiter";
            }
        }

        [Name(SomeClassificationTypes.Operator), Export]
        internal ClassificationTypeDefinition OperatorClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.Operator)]
        [Name("SomeOperatorFormatDefinition")]
        [Order]
        internal sealed class OperatorClassificationFormat : ClassificationFormatDefinition
        {
            public OperatorClassificationFormat()
            {
                this.DisplayName = "Some Operator";
            }
        }

        [Name(SomeClassificationTypes.Keyword), Export]
        internal ClassificationTypeDefinition KeywordClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.Keyword)]
        [Name("SomeKeywordFormatDefinition")]
        [Order]
        internal sealed class KeywordClassificationFormat : ClassificationFormatDefinition
        {
            internal KeywordClassificationFormat()
            {
                ForegroundColor = Colors.Magenta;
                this.DisplayName = "Some Keyword";
            }
        }

        [Name(SomeClassificationTypes.Identifier), Export]
        internal ClassificationTypeDefinition IdentifierClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.Identifier)]
        [Name("SomeIdentifierFormatDefinition")]
        [Order]
        internal sealed class IdentifierClassificationFormat : ClassificationFormatDefinition
        {
            public IdentifierClassificationFormat()
            {
                this.DisplayName = "Some Identifier";
            }
        }

        [Name(SomeClassificationTypes.TypeName), Export]
        internal ClassificationTypeDefinition SomeTypeClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.TypeName)]
        [Name("SomeTypeNameFormatDefinition")]
        [Order]
        internal sealed class SomeTypeClassificationFormat : ClassificationFormatDefinition
        {
            public SomeTypeClassificationFormat()
            {
                this.ForegroundColor = Colors.Teal;
                this.DisplayName = "Some Type Name";
            }
        }

        [Name(SomeClassificationTypes.String), Export]
        internal ClassificationTypeDefinition StringClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.String)]
        [Name("SomeStringFormatDefinition")]
        [Order]
        internal sealed class StringClassificationFormat : ClassificationFormatDefinition
        {
            internal StringClassificationFormat()
            {
                ForegroundColor = Colors.Brown;
                this.DisplayName = "Some String";
            }
        }

        [Name(SomeClassificationTypes.Number), Export]
        internal ClassificationTypeDefinition NumberClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.Number)]
        [Name("SomeNumberFormatDefinition")]
        [Order]
        internal sealed class NumberClassificationFormat : ClassificationFormatDefinition
        {
            public NumberClassificationFormat()
            {
                ForegroundColor = Colors.Magenta;
                this.DisplayName = "Some Number";
            }
        }

        [Name(SomeClassificationTypes.BasicType), Export]
        internal ClassificationTypeDefinition BasicTypeClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.BasicType)]
        [Name("SomeBasicTypeFormatDefinition")]
        [Order]
        internal sealed class BasicTypeClassificationFormat : ClassificationFormatDefinition
        {
            public BasicTypeClassificationFormat()
            {
                ForegroundColor = Colors.Blue;
                this.DisplayName = "Some Basic Type";
            }
        }


        [Name(SomeClassificationTypes.Unknown), Export]
        internal ClassificationTypeDefinition UnknownClassificationType { get; set; }

        [Export(typeof(EditorFormatDefinition))]
        [UserVisible(true)]
        [ClassificationType(ClassificationTypeNames = SomeClassificationTypes.Number)]
        [Name("SomeUnknownFormatDefinition")]
        [Order]
        internal sealed class UnknownClassificationFormat : ClassificationFormatDefinition
        {
            public UnknownClassificationFormat()
            {
                this.DisplayName = "Some Unknown";
            }
        }
    }
}