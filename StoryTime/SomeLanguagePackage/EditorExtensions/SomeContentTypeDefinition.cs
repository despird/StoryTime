using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace SOME.SomeLanguageService.EditorExtensions
{
    /// <summary>
    /// Exports the iron python content type and file extension
    /// </summary>
    public sealed class SomeContentTypeDefinition
    {
        public const string ContentType1 = "SMR";
        public const string ContentType2 = "SMS";

        /// <summary>
        /// Exports the ISome content type
        /// </summary>
        [Export(typeof(ContentTypeDefinition))]
        [Name(SomeContentTypeDefinition.ContentType1)]
        [BaseDefinition("code")]
        public ContentTypeDefinition ISomeContentType1 { get; set; }

        [Export(typeof(ContentTypeDefinition))]
        [Name(SomeContentTypeDefinition.ContentType2)]
        [BaseDefinition("code")]
        public ContentTypeDefinition ISomeContentType2 { get; set; }

        /// <summary>
        /// Exports the ISome file extension
        /// </summary>
		[Export(typeof(FileExtensionToContentTypeDefinition))]
		[ContentType(SomeContentTypeDefinition.ContentType2)]
        [FileExtension(".sms")]
        public FileExtensionToContentTypeDefinition ISomeFileExtension1 { get; set; }

        /// <summary>
        /// Exports the ISome file extension
        /// </summary>
        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(SomeContentTypeDefinition.ContentType1)]
        [FileExtension(".smr")]
        public FileExtensionToContentTypeDefinition ISomeFileExtension2 { get; set; }

                /// <summary>
        ///// Exports the ISome file extension
        ///// </summary>
        //[Export(typeof(FileExtensionToContentTypeDefinition))]
        //[ContentType(SomeContentTypeDefinition.ContentType)]
        //[FileExtension(".som")]
        //public FileExtensionToContentTypeDefinition ISomeFileExtension3 { get; set; }

    }
}