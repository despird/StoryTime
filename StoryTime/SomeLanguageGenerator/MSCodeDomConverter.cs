using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using SOME.SomeLanguageCore.CodeDom;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageGenerator
{
    //http://msdn.microsoft.com/en-us/library/system.codedom.compiler.codedomprovider.aspx
    public class MSCodeDomConverter
    {
        #region all classes in single file
        public static CodeCompileUnit ConvertSomeTreeToCodeDom(SomeDom someTree)
        {
            CodeCompileUnit unit = new CodeCompileUnit();
            CodeNamespace[] name_spaces = GetNamespaces(someTree);
            unit.Namespaces.AddRange(name_spaces);
            return unit;
        }

        private static CodeNamespace[] GetNamespaces(SomeDom someTree)
        {
            string[] nsNames = someTree.GetNameSpaces();

            List<CodeNamespace> codeNamespaces = new List<CodeNamespace>();
            foreach (string nsName in nsNames)
            {
                CodeNamespace ns = new CodeNamespace(nsName);
                codeNamespaces.Add(ns);
                foreach (SomeClass typeSome in someTree.Classes)
                {
                    if (typeSome.NameSpace == nsName)
                    {
                        CodeTypeDeclaration typeDom = GetType(typeSome);
                        ns.Types.Add(typeDom);
                    }
                }
            }
            return codeNamespaces.ToArray();
        }
        #endregion

        #region one file one class
        public static List<CodeCompileUnit> ConvertSomeTreeToCodeDomMultiFile(SomeDom someTree)
        {
            List<CodeCompileUnit> units = new List<CodeCompileUnit>();
            List<SomeClass> types = someTree.Classes;

            foreach (SomeClass type in types)
            {
                CodeCompileUnit unit = new CodeCompileUnit();
                CodeNamespace ns = new CodeNamespace(type.NameSpace);
                ns.Types.Add(GetType(type));
                ns.Imports.Add(new CodeNamespaceImport("System"));
                ns.Imports.Add(new CodeNamespaceImport("System.Text"));
                ns.Imports.Add(new CodeNamespaceImport("System.Collections.Generic"));
                ns.Imports.Add(new CodeNamespaceImport("System.IO"));

                unit.Namespaces.Add(ns);
                units.Add(unit);
            }

            return units;
        }
        #endregion

        private static CodeTypeDeclaration GetType(SomeClass typeSome)
        {
            CodeTypeDeclaration typeDom = new CodeTypeDeclaration(typeSome.ClassName);

            GetClassType(typeSome, typeDom);

            GetBaseTypes(typeSome, typeDom);

            GetMembers(typeSome, typeDom);

            return typeDom;
        }

        private static void GetMembers(SomeClass typeSome, CodeTypeDeclaration typeDom)
        {
            //Fields
            if (typeSome.Fields != null)
            {
                foreach (SomeField fld in typeSome.Fields)
                {
                    CodeMemberField fldMember = new CodeMemberField(SomeBasicTypes.TyeGetBasicTypeName(fld.Type), fld.Name);

                    GetMemberAttributes(fld, fldMember);

                    typeDom.Members.Add(fldMember);
                }
            }

            //Properties
            if (typeSome.Properties != null)
            {
                foreach (SomeProperty prop in typeSome.Properties)
                {
                    CodeMemberProperty propMember = new CodeMemberProperty();

                    propMember.Name = prop.Name;
                    propMember.Type = new CodeTypeReference(SomeBasicTypes.TyeGetBasicTypeName(prop.Type));
                    propMember.HasGet = prop.IsGettable;
                    propMember.HasSet = prop.IsSettable;

                    GetMemberAttributes(prop, propMember);

                    typeDom.Members.Add(propMember);
                }
            }

            //Methods
            if (typeSome.Methods != null)
            {
                foreach (SomeMethod method in typeSome.Methods)
                {
                    CodeMemberMethod methMember = null;

                    if (method.IsConstructor)
                    {
                        methMember = new CodeConstructor();
                    }
                    else if (method.IsDestructor)
                    {
                        //TODO: Special treatment for destructor
                        continue;
                    }
                    else
                    {
                        methMember = new CodeMemberMethod();
                    }


                    methMember.Name = method.Name;

                    //methMember.ReturnType = new CodeTypeReference("");
                    methMember.ReturnType = new CodeTypeReference(SomeBasicTypes.TyeGetBasicTypeName(method.Type));


                    //Parameters
                    GetMethodParameters(method, methMember);

                    //Statements TODO: Uncomment
                    GetMethodStatements(method, methMember);

                    GetMemberAttributes(method, methMember);

                    typeDom.Members.Add(methMember);
                }
            }
        }

        private static void GetMethodStatements(SomeMethod method, CodeMemberMethod methMember)
        {
            if (method.Statements != null)
            {
                foreach (SomeStatement statement in method.Statements)
                {
                    CodeStatement stateDom = statement.GetMSCodeDomStatement();

                    if (stateDom != null)
                    {
                        methMember.Statements.Add(stateDom);
                    }
                }
            }
        }

        private static void GetMethodParameters(SomeMethod method, CodeMemberMethod methMember)
        {
            if (method.Parameters != null)
            {
                foreach (SomeParameter parameter in method.Parameters)
                {
                    CodeParameterDeclarationExpression paraDom =
                        new CodeParameterDeclarationExpression(SomeBasicTypes.TyeGetBasicTypeName(parameter.Type), parameter.PassedObj);
                    
                    methMember.Parameters.Add(paraDom);
                }
            }
        }

        private static void GetMemberAttributes(SomeTreeNodeMemberBase member, CodeTypeMember domMember)
        {
            switch (member.AccLevel)
            {
                case AccessLevel.Private:
                    domMember.Attributes = MemberAttributes.Private;
                    break;
                case AccessLevel.Protected:
                    domMember.Attributes = MemberAttributes.Family;
                    break;
                case AccessLevel.Public:
                    domMember.Attributes = MemberAttributes.Public;
                    break;
            }

            domMember.Attributes &= ~MemberAttributes.ScopeMask;

            if (member.IsAbstract)
            {
                domMember.Attributes |= MemberAttributes.Abstract;
            }
            else if (member.IsConst)
            {
                domMember.Attributes |= MemberAttributes.Const;
            }
            else if (member.IsOverride)
            {
                domMember.Attributes |= MemberAttributes.Override;
            }
            else if (member.IsStatic)
            {
                domMember.Attributes |= MemberAttributes.Static;
            }
            else if (!member.IsVirtual)
            {
                domMember.Attributes |= MemberAttributes.Final;
            }

        }

        private static void GetBaseTypes(SomeClass typeSome, CodeTypeDeclaration typeDom)
        {
            if (typeSome.Parents != null)
            {
                foreach (string parent in typeSome.Parents)
                {
                    typeDom.BaseTypes.Add(parent);
                }
            }
        }

        private static void GetClassType(SomeClass typeSome, CodeTypeDeclaration typeDom)
        {
            switch (typeSome.ClsType)
            {
                case SomeClass.ClassType.Abstract:
                    {
                        typeDom.TypeAttributes = typeDom.TypeAttributes | System.Reflection.TypeAttributes.Abstract;
                    } break;
                case SomeClass.ClassType.Class:
                    break;
                case SomeClass.ClassType.Interface:
                    {
                        typeDom.IsInterface = true;
                    } break;
                case SomeClass.ClassType.Struct:
                    {
                        typeDom.IsStruct = true;
                    } break;
            }
        }

    }
}
