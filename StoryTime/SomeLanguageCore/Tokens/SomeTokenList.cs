using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageCore.Tokens
{
    public class SomeTokenList
    {
        public SomeToken First = null;
        public SomeToken Current = null;

        public void Clear()
        {
            Delete(First);
            First = null;
            Current = null;
        }

        private void Delete(SomeToken last)
        {
            if (last == null)
            {
                return;
            }

            if (last.NextToken != null)
            {
                Delete(last.NextToken);
                last.NextToken = null;
            }
        }

        public void Add(SomeToken token)
        {
            if (Current != null)
            {
                Current.NextToken = token;
            }
            else
            {
                First = token;
            }

            Current = token;
        }
    }
}
