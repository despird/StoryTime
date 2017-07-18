using System;
using System.Collections.Generic;
using System.Text;

namespace SOME.SomeLanguageCore.CodeDom
{
    public abstract class SomeTreeNodeBase
    {
    }

    public enum AccessLevel
    {
        Private,
        Public,
        Protected
    }

    public abstract class SomeTreeNodeMemberBase : SomeTreeNodeBase
    {
        public string Name = "";

        public string Type = "";

        //public bool IsCollection = false;

        public bool IsAbstract = false;

        public bool IsVirtual = false;

        public bool IsOverride = false;

        public bool IsConst = false;

        public bool IsStatic = false;

        public AccessLevel AccLevel = AccessLevel.Private;

    }
}
