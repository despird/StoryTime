using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeExpressionInvoking
    {
        public string InvokingObjectType = "";
        public string InvokingObjectRealType = "";
        public string InvokingObject = "";
        public string StaticInvokingType = "";
        public string Method = "";

        public List<SomeParameter> MethodParameters = null;

        public void AddParameter(SomeParameter parameter)
        {
            if (this.MethodParameters == null)
                this.MethodParameters = new List<SomeParameter>();

            this.MethodParameters.Add(parameter);
        }
    }

}
