using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeStatementMethodInvoking : SomeStatement
    {
        public enum SomeStatementMethodInvokingType
        {
            PrivateInvoking,
            InstanceInvoking,
            StaticInvoking,
        }

        public SomeStatementMethodInvokingType InvokingType;

        SomeExpressionInvoking Invoking = null;

        public SomeStatementMethodInvoking(SomeExpressionInvoking invoking, SomeStatementMethodInvokingType type)
            : base(SomeStatementType.Invoking)
        {
            Invoking = invoking;
            InvokingType = type;
        }

        public override void AddParameter(SomeParameter parameter)
        {
            if (this.Invoking != null)
            {
                this.Invoking.AddParameter(parameter);
            }
        }

        public override string MethodType
        {
            get
            {
                if (Invoking != null)
                {
                    return Invoking.InvokingObjectRealType != string.Empty ? Invoking.InvokingObjectRealType : Invoking.InvokingObjectType;
                }
                return base.MethodType;
            }
        }

        public override string InvokingObject
        {
            get
            {
                if (Invoking != null)
                {
                    return Invoking.InvokingObject;
                }
                return base.InvokingObject;
            }
        }

        public override string MethodName
        {
            get
            {
                if (this.Invoking != null)
                {
                    return this.Invoking.Method;
                }
                return base.MethodName;
            }
        }

        public override List<SomeParameter> Parameters
        {
            get
            {
                if (this.Invoking != null)
                {
                    return this.Invoking.MethodParameters;
                }
                return base.Parameters;
            }
        }

        private CodeMethodInvokeExpression GetCodeMethodInvokeExpression(CodeExpression invokeObj)
        {
            CodeExpression[] parasExp = this.GetParameterExpressions();
            CodeMethodInvokeExpression cm = parasExp != null ? new CodeMethodInvokeExpression(invokeObj, this.MethodName, parasExp) : new CodeMethodInvokeExpression(invokeObj, this.MethodName);
            return cm;
        }

        public CodeMethodInvokeExpression GetInvokeExpression()
        {
            CodeMethodInvokeExpression methInv = null;
            switch (InvokingType)
            {
                case SomeStatementMethodInvokingType.PrivateInvoking:
                    {
                        methInv = GetCodeMethodInvokeExpression(null);
                    } break;
                case SomeStatementMethodInvokingType.InstanceInvoking:
                    {
                        methInv = GetCodeMethodInvokeExpression(new CodeVariableReferenceExpression(this.InvokingObject));
                    } break;
                case SomeStatementMethodInvokingType.StaticInvoking:
                    {
                        methInv = GetCodeMethodInvokeExpression(new CodeTypeReferenceExpression(this.InvokingObject));
                    } break;
                default:
                    break;
            }

            return methInv;
        }

        public override System.CodeDom.CodeStatement GetMSCodeDomStatement()
        {
            return new CodeExpressionStatement(GetInvokeExpression());
        }
    }
}
