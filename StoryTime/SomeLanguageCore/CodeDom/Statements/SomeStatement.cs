using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.CodeDom;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageCore.CodeDom
{
    public enum SomeStatementType
    {
        Creation,
        Invoking,
        Assignment,
        Comment,
        Snippet,
        Return
    }

    public abstract class SomeStatement : SomeTreeNodeBase
    {
        public override string ToString()
        {
            return "TODO";
        }

        private SomeStatementType _statmentType;

        public SomeStatement(SomeStatementType statType)
        {
            this._statmentType = statType;
        }

        #region virtul properties
        public virtual string DefType
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual string DefRealType
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual string DefObject
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual string InvokingObject
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual string MethodType
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual string MethodName
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual List<SomeParameter> Parameters
        {
            get
            {
                return null;
            }
        }

        #endregion

        public SomeStatementType StatmentType
        {
            get
            {
                return _statmentType;
            }
        }

        public virtual void AddParameter(SomeParameter parameter)
        {
            throw new Exception("must be overriden");
        }

        public virtual System.CodeDom.CodeStatement GetMSCodeDomStatement()
        {
            CodeExpression invokeExpression = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression(typeof(Console)),
                "Write", new CodePrimitiveExpression("Statement Not Implemented!"));

            // Creates a statement using a code expression.
            return new CodeExpressionStatement(invokeExpression);
        }

        protected CodeExpression[] GetParameterExpressions()
        {
            if (this.Parameters != null)
            {
                List<CodeExpression> codeExps = new List<CodeExpression>();
                foreach (SomeParameter para in this.Parameters)
                {
                    codeExps.Add(new CodeVariableReferenceExpression(para.PassedObj));
                }
                return codeExps.ToArray();
            }
            return null;
        }

        protected CodeExpression GetObjectOrPropertyReference(SomeExpressionObject expObj)
        {
            if (string.IsNullOrEmpty(expObj.ObjectOfObject))
            {
                return new CodeVariableReferenceExpression(expObj.Object);
            }
            else
            {
                return new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(expObj.Object), expObj.ObjectOfObject);
            }
        }

    }
}
