using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeStatementReturn : SomeStatement
    {
        public enum SomeStatementReturnType
        {
            ReturnConstant,
            ReturnObject,
            ReturnConstrution
        }
        public SomeStatementReturnType ReturnType;

        //TODOF: This should be replaced with an object type to support CodePrimitiveExpression generation
        public string ReturnConstantObject = "";
        /*
         * public enum PrimativeType
         * {
         *      Integer,
         *      Float,
         *      String,
         *      Char,
         *      Null,
         * }
         */

        public SomeExpressionObject ReturnObject = null;
        public string ConstructionType = "";
        public List<SomeParameter> ConstructionParameters = null;

        public SomeStatementReturn(string constant, SomeExpressionObject return_obj, string construct_type, /*List<SomeParameter> parameters,*/ SomeStatementReturnType return_type)
            : base(SomeStatementType.Return)
        {
            ReturnConstantObject = constant;
            ReturnObject = return_obj;
            ConstructionType = construct_type;
            ReturnType = return_type;
        }

        public override void AddParameter(SomeParameter parameter)
        {
            if (this.ConstructionParameters == null)
            {
                this.ConstructionParameters = new List<SomeParameter>();
            }

            this.ConstructionParameters.Add(parameter);
        }

        public override string MethodType
        {
            get
            {
                if (ConstructionType != string.Empty)
                {
                    return ConstructionType;
                }
                return base.MethodType;
            }
        }

        public override string MethodName
        {
            get
            {
                if (ConstructionType != string.Empty)
                {
                    return ConstructionType;
                }
                return base.MethodName;
            }
        }

        public override List<SomeParameter> Parameters
        {
            get
            {
                return ConstructionParameters;
            }
        }

        private CodeObjectCreateExpression GetObjectCreationExpression()
        {

            CodeExpression[] parasExp = this.GetParameterExpressions();
            CodeObjectCreateExpression ce = parasExp != null ? new CodeObjectCreateExpression(SomeBasicTypes.TyeGetBasicTypeName(this.ConstructionType), parasExp) : new CodeObjectCreateExpression(this.ConstructionType);
            return ce;
        }

        public override System.CodeDom.CodeStatement GetMSCodeDomStatement()
        {
            switch (ReturnType)
            {
                case SomeStatementReturnType.ReturnObject:
                    {
                        CodeExpression exp = GetObjectOrPropertyReference(this.ReturnObject);
                        return new CodeMethodReturnStatement(exp);
                    }
                case SomeStatementReturnType.ReturnConstant:
                    {
                        //TODOF: should be implemented with CodePrimitiveExpression
                        CodeSnippetExpression exp = new CodeSnippetExpression(this.ReturnConstantObject);
                        return new CodeMethodReturnStatement(exp);
                    }
                case SomeStatementReturnType.ReturnConstrution:
                    {
                        CodeObjectCreateExpression cons = GetObjectCreationExpression();
                        return new CodeMethodReturnStatement(cons);
                    }
                default:
                    break;
            }
            return base.GetMSCodeDomStatement();
        }

    }
}
