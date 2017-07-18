using System;
using System.Collections.Generic;
using System.Text;
using SOME.SomeLanguageCore.Parser;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeProperty : SomeTreeNodeMemberBase
    {
        public bool IsGettable = true;
        public bool IsSettable = true;

        public string AccFieldName = "";

        public SomeProperty()
        {
            this.AccLevel = AccessLevel.Public;
        }

        public SomeProperty(SomeField field): this()
        {
            CopyFromField(field);
        }

        public void CopyFromField(SomeField field)
        {
            this.IsAbstract = field.IsAbstract;
            //this.IsCollection = field.IsCollection;
            this.IsConst = field.IsConst;
            this.IsOverride = field.IsOverride;
            this.IsStatic = field.IsStatic;
            this.Type = field.Type;
            this.Name = SomeParser.MakeFirstUpper(field.Name.Substring(1));
            this.AccFieldName = field.Name;
        }

    }
}
