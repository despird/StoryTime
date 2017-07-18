using System;
using System.Collections.Generic;
using System.Text;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeField : SomeTreeNodeMemberBase
    {
        public bool IsInitialized = false;

        public string InitialCreationType = "";

        public string InitialCreationParaString = "";

        private SomeClass _someType;

        public SomeClass SomeType
        {
            get { return _someType; }
            set { _someType = value; }
        }

        public SomeField()
        {
            InitialCreationType = this.Type;
        }
    }
}
