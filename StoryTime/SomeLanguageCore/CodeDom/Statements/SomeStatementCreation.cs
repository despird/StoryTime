using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeStatementCreation : SomeStatement
    {
        public enum SomeStatementCreationType
        {
            Defination,
            Construction,
            DefinationWithConstruction
        }

        public override string MethodType
        {
            get
            {
                if (Construction != null)
                {
                    return Construction.ConstructionObjectRealType != string.Empty ? Construction.ConstructionObjectRealType : Construction.ConstructionObjectType;
                }
                return base.MethodType;
            }
        }

        public override string MethodName
        {
            get
            {
                if (Construction != null)
                {
                    return Construction.ConstructionObjectRealType != string.Empty ? Construction.ConstructionObjectRealType : Construction.ConstructionObjectType;
                }
                return base.MethodName;
            }
        }

        public override string DefType
        {
            get
            {
                if (this.Defination != null)
                {
                    return Defination.DefinationType;
                }
                return base.DefType;
            }
        }

        public override string DefRealType
        {
            get
            {
                if (this.Construction != null)
                {
                    return Construction.ConstructionObjectRealType;
                }

                return this.DefType;
            }
        }

        public override string DefObject
        {
            get
            {
                if (this.Defination != null)
                {
                    return Defination.DefinationObject;
                }
                else if (this.Construction != null)
                {
                    return Construction.ConstructionObject;
                }
                return base.DefObject;
            }
        }

        public override List<SomeParameter> Parameters
        {
            get
            {
                if (this.Construction != null)
                {
                    return Construction.ConstructorParameters;
                }
                return base.Parameters;
            }
        }


        public SomeExpressionDefination Defination = null;
        public SomeExperessionConstruction Construction = null;
        public SomeStatementCreationType CreationType;

        public SomeStatementCreation(SomeExpressionDefination defination, SomeExperessionConstruction construction, SomeStatementCreationType creation_type)
            : base(SomeStatementType.Creation)
        {
            Defination = defination;
            Construction = construction;
            CreationType = creation_type;
        }

        public override void AddParameter(SomeParameter parameter)
        {
            if (this.Construction != null)
            {
                this.Construction.AddParameter(parameter);
            }
        }

        private CodeObjectCreateExpression GetObjectCreationExpression()
        {

            CodeExpression[] parasExp = this.GetParameterExpressions();
            CodeObjectCreateExpression ce = parasExp != null ? new CodeObjectCreateExpression(SomeBasicTypes.TyeGetBasicTypeName(this.DefRealType), parasExp) : new CodeObjectCreateExpression(this.DefRealType);
            return ce;
        }

        public override System.CodeDom.CodeStatement GetMSCodeDomStatement()
        {
            switch (CreationType)
            {
                case SomeStatementCreationType.Defination:
                    {
                        CodeVariableDeclarationStatement cs = new CodeVariableDeclarationStatement(SomeBasicTypes.TyeGetBasicTypeName(this.DefType), this.DefObject);
                        return cs;
                    }
                case SomeStatementCreationType.Construction:
                    {
                        CodeVariableReferenceExpression left = new CodeVariableReferenceExpression(this.DefObject);
                        CodeObjectCreateExpression right = GetObjectCreationExpression();
                        CodeAssignStatement cs = new CodeAssignStatement(left, right);
                        return cs;
                    }
                case SomeStatementCreationType.DefinationWithConstruction:
                    {
                        CodeObjectCreateExpression ce = GetObjectCreationExpression();
                        CodeVariableDeclarationStatement cs = new CodeVariableDeclarationStatement(SomeBasicTypes.TyeGetBasicTypeName(this.DefType), this.DefObject, ce);
                        return cs;
                    }
                default:
                    break;
            }

            return base.GetMSCodeDomStatement();
        }
    }
}
