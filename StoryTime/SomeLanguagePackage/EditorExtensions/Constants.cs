using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageService.EditorExtensions
{
    internal static class Constants
    {
        /// <summary>
        /// The Guid of the IPy language service
        /// </summary>
        internal static readonly Guid SomeLanguageServiceGuid = new Guid("{6E0A6C63-7AC6-49E2-88FA-61FA9D1D2FCC}");

        /// <summary>
        /// Word separators chars
        /// </summary>
        internal static char[] Separators = new[] { '\n', '\r', '\t', ' ', ':', '(', ')', '[', ']', '{', '}', '?', '/', '+', '-', ';', '=', '*', '!', ',', '<', '>' };

        /// <summary>
        /// Word separators chars including dot
        /// </summary>
        internal static char[] SeparatorsPlusDot = Separators.Union(new[] {'.'}).ToArray();
    }
}
