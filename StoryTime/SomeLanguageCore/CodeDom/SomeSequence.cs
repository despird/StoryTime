using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using SOME.SomeLanguageCore.Tokens;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeSequence : SomeTreeNodeBase
    {
        public SomeSequence(SomeToken begin, SomeToken end)
        {
            _seqStr = begin.TokenString;

            if (begin != end)
            {
                while (begin != end && begin.Next != null)
                {
                    begin = begin.Next;
                    _seqStr += begin.TokenString;
                }
            }
        }

        public SomeSequence()
        {
        }

        public SomeSequence(SomeStatement attach)
        {
            this._attachedStatement = attach;
            _seqStr = attach.ToString();
        }

        private string _seqStr;

        public override string ToString()
        {
            return _seqStr;
        }

        private List<SomeSequence> _subSequences = null;

        //private List<SomeParameter> _parameters = null;

        private List<SomeStatement> _statements = null;

        public List<SomeStatement> Statements
        {
            get
            {
                if (_statements == null)
                    _statements = new List<SomeStatement>();

                return _statements;
            }
        }

        //private List<SomeSequenceLocal> _locals = null;
        //public List<SomeSequenceLocal> Locals
        //{
        //    get
        //    {
        //        return _locals;
        //    }
        //}

        private SomeSequence _parentSequence = null;

        public SomeSequence ParentSequence
        {
            get { return _parentSequence; }
            set { _parentSequence = value; }
        }

        //public void AddLocals(SomeSequenceLocal local)
        //{
        //    if (_locals == null)
        //        _locals = new List<SomeSequenceLocal>();

        //    _locals.Add(local);
        //}

        public void AddSubSequence(SomeSequence sub)
        {
            if (_subSequences == null)
                _subSequences = new List<SomeSequence>();

            _subSequences.Add(sub);
            sub.ParentSequence = this;
        }

        public void AddStatement(SomeStatement statement)
        {
            if (_statements == null)
                _statements = new List<SomeStatement>();

            _statements.Add(statement);
        }

        public void ParseStatements(List<SomeClass> classes)
        {
            //find the method
            SomeMethod method = lookupTypeMethod(classes);

            if (method != null)
            {
                method.Statements = this.Statements;
            }

            if (_subSequences != null)
            {
                foreach (SomeSequence sub in _subSequences)
                {
                    sub.ParseStatements(classes);
                }
            }
        }

        private SomeMethod lookupTypeMethod(List<SomeClass> classes)
        {
            string type_name = this.AttachedStatement.MethodType;
            string method_name = this.AttachedStatement.MethodName;

            SomeClass cls = lookupType(classes);

            if (cls != null)
            {
                SomeMethod method = cls.LookupTypeMethod(this, method_name);
                if (method != null)
                {
                    return method;
                }
            }

            //Error
            SomeErrorReporter.AddError(string.Format("Method not found:{0}.{1}", type_name, method_name));
            return null;
        }

        private SomeClass lookupType(List<SomeClass> classes)
        {
            string type_name = this.AttachedStatement.MethodType;
            return classes.Find(cls => cls.ClassName == type_name);
        }

        public bool CheckParameters(SomeMethod method)
        {
            List<SomeParameter> parameters = AttachedStatement.Parameters;

            if (parameters == null && method.Parameters == null)
            {
                return true;
            }

            if (parameters.Count != method.Parameters.Count)
            {
                return false;
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                //if parameters belong to the root sequence, it may not have type name
                //CClient.main(arg[]){...}

                method.Parameters[i].PassedObj = parameters[i].PassedObj;

                if (SomeBasicTypes.TryParseBasicType(method.Parameters[i].Type, parameters[i].InvokingObj))
                {
                    continue;
                }

                if (parameters[i].Type == string.Empty)
                {
                    parameters[i].Type = method.Parameters[i].Type;
                }

                if (parameters[i].Type != method.Parameters[i].Type)
                {
                    if (!SomeCompiler.checkIsDesendent(parameters[i].Type, method.Parameters[i].Type))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public string LookupObjTypeFromSequence(string obj_name)
        {
            string obj_type = obj_name;

            if (lookupObjTypeFromSequenceStatements(obj_name, ref obj_type)) return obj_type;
            if (lookupObjTypeFromSequenceParameters(obj_name, ref obj_type)) return obj_type;
            if (lookupObjTypeFromSequenceAttachedTypeMembers(obj_name, ref obj_type)) return obj_type;

            return obj_type;
        }

        private bool lookupObjTypeFromSequenceStatements(string obj_name, ref string type)
        {
            //From statement
            if (Statements == null)
            {
                return false;
            }

            foreach (SomeStatement statement in Statements)
            {
                SomeStatementCreation creation = statement as SomeStatementCreation;
                if (creation != null)
                {
                    if (creation.DefObject == obj_name)
                    {
                        if (creation.DefType != string.Empty)
                        {
                            type = creation.DefType;
                            return true;
                        }
                        else if (creation.MethodType != string.Empty)
                        {
                            type = creation.MethodType;
                            return true;
                        }
                    }
                    continue;
                }

                SomeStatementAssignment assign = statement as SomeStatementAssignment;
                if (assign != null)
                {
                    if (assign.LeftDefination != null)
                    {
                        if (assign.DefObject == obj_name)
                        {
                            type = assign.DefRealType;
                            return true;
                        }
                    }
                    continue;
                }
            }

            return false;
        }

        private bool lookupObjTypeFromSequenceParameters(string obj_name, ref string type)
        {
            if (AttachedStatement == null || AttachedStatement.Parameters == null)
            {
                return false;
            }

            foreach (SomeParameter para in AttachedStatement.Parameters)
            {
                if (para.PassedObj == obj_name)
                {
                    type = para.Type;
                    return true;
                }
            }

            return false;
        }

        private bool lookupObjTypeFromSequenceAttachedTypeMembers(string obj_name, ref string type)
        {
            if (AttachedStatement == null)
            {
                return false;
            }

            string inv_type = AttachedStatement.MethodType;
            foreach (SomeClass cls in SomeCompiler.CodeDom.Classes)
            {
                if (cls.ClassName == inv_type)
                {
                    return cls.LookupMemberObjType(obj_name, ref type);
                }
            }

            return false;
        }

        private SomeStatement _attachedStatement = null;
        public SomeStatement AttachedStatement
        {
            get
            {
                return _attachedStatement;
            }
            set
            {
                _attachedStatement = value;
            }
        }


        public string LookupAttachedInvokingType(int line_num)
        {
            if (this.AttachedStatement == null)
            {
                throw new Exception(string.Format("{0}:cannot lookup type of invoking object, its parent's attached statement is null!", line_num));
            }

            return AttachedStatement.MethodType;
        }

        public List<string> LocalsOrMembers
        {
            get
            {
                //locals
                List<string> objectNames = this.Locals;

                //members, todo: add those from parents
                SomeClass cls = lookupType(SomeCompiler.Classes);
                if (cls != null)
                {
                    objectNames.AddRange(cls.Members);
                }

                return objectNames.Distinct().ToList();
            }
        }

        public List<string> LocalsOrMemberObjects
        {
            get
            {
                //locals
                List<string> objectNames = this.Locals;

                //members, todo: add those from parents
                SomeClass cls = lookupType(SomeCompiler.Classes);
                if (cls != null)
                {
                    objectNames.AddRange(cls.MemberFields);
                }

                return objectNames.Distinct().ToList();
            }
        }

        public List<string> Locals
        {
            get
            {
                //locals
                List<string> localNames = new List<string>();
                Statements.ForEach(statement =>
                    {
                        string defObj = statement.DefObject;
                        if (!string.IsNullOrEmpty(defObj) && !localNames.Contains(defObj))
                        {
                            localNames.Add(defObj);
                        }
                    });

                //examine parent sequence(s)
                if (this.ParentSequence != null)
                {
                    localNames.AddRange(this.ParentSequence.Locals);
                }

                return localNames.Distinct().ToList();
            }
        }
    }
}

