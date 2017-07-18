using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageCore.Tokens
{
    public class SomeChar
    {
        public const char Tab = '\t';
        public const char Blank = ' ';
        public const char Dot = '.';
        public const char Colon = ':';
        public const char Semi = ';';
        public const char LBracket = '[';
        public const char RBracket = ']';
        public const char LCurly = '{';
        public const char RCurly = '}';
        public const char LParen = '(';
        public const char RParen = ')';
        public const char Percent = '%';
        public const char ULine = '_';
        public const char Equal = '=';
        public const char Less = '<';
        public const char Minus = '-';
        public const char Greater = '>';
        public const char Comma = ',';
        public const char Slash = '/';
        public const char Star = '*';
        public const char Tilde = '~';
        public const char Quote = '"';
        public const char Return = '\r';
        public const char NewLine = '\n';

        public static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        public static bool IsLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        }

        public static bool IsDigitOrLetter(char c)
        {
            return IsDigit(c) || IsLetter(c);
        }

        public static bool IsIdenCharStart(char c)
        {
            return IsLetter(c) || c == SomeChar.ULine;
        }

        public static bool IsIdentChar(char c)
        {
            return IsDigitOrLetter(c) || c == SomeChar.ULine;
        }

        public static bool IsTypeStartChar(char c)
        {
            return (c == 'C' || c == 'A' || c == 'I' || c == 'S');
        }
    }
}
