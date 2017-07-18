using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageGenerator
{
    public class SomeStringBuilder
    {
        StringBuilder _sb;
        int _indentTimes = 0;

        public SomeStringBuilder()
            : this(0)
        {
        }

        public SomeStringBuilder(int indent_times)
        {
            _sb = new StringBuilder();
            this._indentTimes = indent_times;
        }

        public SomeStringBuilder(string str, int indent_times)
        {
            _sb = new StringBuilder(str);
            this._indentTimes = indent_times;
        }

        public StringBuilder Append(string str)
        {
            return _sb.Append(str);
        }

        public StringBuilder Remove(int startIndex, int length)
        {
            return _sb.Remove(startIndex, length);
        }

        public int Length
        {
            get
            {
                return _sb.Length;
            }
        }

        public StringBuilder AppendLine()
        {
            _sb.AppendLine();
            for (int i = 0; i < _indentTimes; i++)
            {
                _sb.Append("    ");
            }
            return _sb;
        }

        public StringBuilder AppendCurlyBegin()
        {
            _sb = AppendLine();
            _sb.Append("{");
            _indentTimes++;
            _sb = AppendLine();
            return _sb;
        }

        public StringBuilder AppendCurlyClose()
        {
            _indentTimes--;
            _sb = AppendLine();
            _sb.Append("}");
            _sb = AppendLine();
            return _sb;
        }

        public StringBuilder Replace(string oldValue, string newValue, int startIndex, int count)
        {
            return _sb.Replace(oldValue, newValue, startIndex, count);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

    }
}
