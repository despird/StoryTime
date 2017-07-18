using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.CodeDom;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeStatementAssignment : SomeStatement
    {
        public enum SomeStatementAssignmentType
        {
            LeftObjEqualsRightObj,
            LeftObjEqualsRightInvoking,
            LeftDefEqualsRightObj,
            LeftDefEqualsRightInvoking
        }

        public SomeStatementAssignmentType AssignmentType;
        public SomeExpressionObject LeftObject = null;
        public SomeExpressionObject RightObject = null;

        public SomeExpressionDefination LeftDefination = null;
        public SomeStatementMethodInvoking RightMethodInvoking = null;

        public SomeStatementAssignment(SomeExpressionObject left_obj, SomeExpressionObject right_obj, SomeExpressionDefination left_def, SomeStatementMethodInvoking right_inv, SomeStatementAssignmentType assign_type)
            : base(SomeStatementType.Assignment)
        {
            LeftObject = left_obj;
            RightObject = right_obj;
            LeftDefination = left_def;
            RightMethodInvoking = right_inv;
            AssignmentType = assign_type;
        }

        public override void AddParameter(SomeParameter parameter)
        {
            if (this.RightMethodInvoking != null)
            {
                this.RightMethodInvoking.AddParameter(parameter);
            }
        }

        public override string DefType
        {
            get
            {
                if (this.LeftDefination != null)
                {
                    return LeftDefination.DefinationType;
                }
                return base.DefType;
            }
        }

        public override string DefRealType
        {
            get
            {
                return this.DefType;
            }
        }

        public override string DefObject
        {
            get
            {
                if (this.LeftDefination != null)
                {
                    return this.LeftDefination.DefinationObject;
                }
                return base.DefObject;
            }
        }

        public override string InvokingObject
        {
            get
            {
                if (this.RightMethodInvoking != null)
                {
                    return RightMethodInvoking.InvokingObject;
                }
                return base.InvokingObject;
            }
        }

        public override string MethodName
        {
            get
            {
                if (this.RightMethodInvoking != null)
                {
                    return RightMethodInvoking.MethodName;
                }
                return base.MethodName;
            }
        }

        public override List<SomeParameter> Parameters
        {
            get
            {
                if (this.RightMethodInvoking != null)
                {
                    return RightMethodInvoking.Parameters;
                }
                return base.Parameters;
            }
        }

        public override string MethodType
        {
            get
            {
                if (this.RightMethodInvoking != null)
                {
                    return this.RightMethodInvoking.MethodType;
                }
                return base.MethodType;
            }
        }

        public override System.CodeDom.CodeStatement GetMSCodeDomStatement()
        {
            switch (AssignmentType)
            {
                case SomeStatementAssignmentType.LeftObjEqualsRightObj:
                    {
                        CodeExpression left = GetObjectOrPropertyReference(this.LeftObject);
                        CodeExpression right = GetObjectOrPropertyReference(this.RightObject);
                        return new CodeAssignStatement(left, right);
                    }
                case SomeStatementAssignmentType.LeftObjEqualsRightInvoking:
                    {
                        CodeExpression left = GetObjectOrPropertyReference(this.LeftObject);
                        CodeExpression right = this.RightMethodInvoking.GetInvokeExpression();
                        return new CodeAssignStatement(left, right);
                    }
                case SomeStatementAssignmentType.LeftDefEqualsRightObj:
                    {
                        CodeExpression initExp = GetObjectOrPropertyReference(this.RightObject);
                        return new CodeVariableDeclarationStatement(SomeBasicTypes.TyeGetBasicTypeName(this.LeftDefination.DefinationType), this.LeftDefination.DefinationObject, initExp);
                    }
                case SomeStatementAssignmentType.LeftDefEqualsRightInvoking:
                    {
                        CodeExpression initExp = this.RightMethodInvoking.GetInvokeExpression();
                        return new CodeVariableDeclarationStatement(SomeBasicTypes.TyeGetBasicTypeName(this.LeftDefination.DefinationType), this.LeftDefination.DefinationObject, initExp);
                    }
                default:
                    break;
            }
            return base.GetMSCodeDomStatement();
        }


    }
}
