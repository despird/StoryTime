using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SOME.SomeLanguageCore.CodeDom
{
    public partial class SomeClass : SomeTreeNodeBase
    {
        public enum ClassType
        {
            Class,
            Interface,
            Abstract,
            Struct
        }

        private string _namespace;

        public string NameSpace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        private List<SomeClass> _parentClasses = null;

        public List<SomeClass> ParentClasses
        {
            get { return _parentClasses; }
            set { _parentClasses = value; }
        }

        private string _className = null;

        private List<string> _parents = null;

        private List<SomeField> _fields = null;

        private List<SomeMethod> _methods = null;

        private List<SomeProperty> _properties = null;

        private ClassType _clsType;

        public ClassType ClsType
        {
            get
            {
                return _clsType;
            }
        }

        public List<string> Parents
        {
            get
            {
                return _parents;
            }
        }

        public List<SomeClass> Ancestors
        {
            get
            {
                List<SomeClass> ancestors = new List<SomeClass>();
                searchAncestors(ancestors, this);
                return ancestors;
            }
        }

        private List<SomeClass> subClasses;

        public List<SomeClass> SubClasses
        {
          get 
          {
              if(subClasses == null) 
                  subClasses = new List<SomeClass>();

                  return subClasses;
          }
        }

        public List<SomeClass> Decendants
        {
            get 
            {
                List<SomeClass> decendants = new List<SomeClass>();
                searchDecendants(decendants, this);
                return decendants;
            }
        }

        public List<string> MemberFields
        {
            get
            {
                List<String> members = new List<string>();
                Fields.ForEach(field => members.Add(field.Name));
                Properties.ForEach(prop => members.Add(prop.Name));

                //inherited members
                this.Ancestors.ForEach(cls => members.AddRange(cls.MemberFields));
                return members.Distinct().ToList();
            }
        }

        public List<string> MemberMethods
        {
            get
            {
                List<String> members = new List<string>();
                Methods.ForEach(
                    method =>
                    {
                        if (!method.IsConstructor)
                        {
                            members.Add(method.Name);
                        }
                    });

                //inherited members
                this.Ancestors.ForEach(cls => members.AddRange(cls.MemberFields));
                return members.Distinct().ToList();
            }
        }

        public List<string> Members
        {
            get
            {
                List<String> members = new List<string>();
                members.AddRange(MemberFields);
                members.AddRange(MemberMethods);
                return members;
            }
        }

        public List<SomeMethod> MethodsToOverride
        {
            get
            {
                List<SomeClass> ancestors = Ancestors;
                List<SomeMethod> methods = new List<SomeMethod>();
                if (ancestors.Count != 0)
                {
                    ancestors.ForEach(ancestor =>
                        {
                            ancestor.Methods.ForEach(method =>
                            {
                                if (!methods.Any(m => m.Name == method.Name))
                                {
                                    methods.Add(method);
                                }
                            });
                        });
                }

                return methods;
            }
        }

        private void searchAncestors(List<SomeClass> ancestors, SomeClass someCls)
        {
            List<SomeClass> parents = someCls.ParentClasses;
            if (parents != null)
            {
                foreach (SomeClass parent in parents)
                {
                    if (!ancestors.Contains(parent))
                    {
                        ancestors.Add(parent);
                    }
                    searchAncestors(ancestors, parent);
                }
            }
        }

        private void searchDecendants(List<SomeClass> decendants, SomeClass someCls)
        {
            List<SomeClass> subs = someCls.SubClasses;
            if (subs != null)
            {
                foreach (SomeClass sub in subs)
                {
                    if (!decendants.Contains(sub))
                    {
                        decendants.Add(sub);
                    }
                    searchDecendants(decendants, sub);
                }
            }
        }

        public override string ToString()
        {
            return this.ClassName;
        }

        public SomeClass(string clsName, string ns)
        {
            _className = clsName;
            _namespace = ns[0].ToString().ToUpper() + ns.Substring(1);
            switch (clsName[0])
            {
                case 'C':
                    _clsType = ClassType.Class;
                    break;
                case 'A':
                    _clsType = ClassType.Abstract;
                    break;
                case 'I':
                    _clsType = ClassType.Interface;
                    break;
                case 'S':
                    _clsType = ClassType.Struct;
                    break;
                default:
                    SomeErrorReporter.AddError("unknown type: " + clsName);
                    _clsType = ClassType.Class;
                    break;
            }
        }

        public string ClassName
        {
            get
            {
                return _className;
            }
        }

        public List<SomeField> Fields
        {
            get
            {
                if (_fields == null)
                    _fields = new List<SomeField>();

                return _fields;
            }
        }

        public List<SomeMethod> Methods
        {
            get
            {
                if (_methods == null)
                    _methods = new List<SomeMethod>();

                return _methods;
            }
        }

        public List<SomeProperty> Properties
        {
            get
            {
                if (_properties == null)
                    _properties = new List<SomeProperty>();

                return _properties;
            }
        }

        public void AddField(SomeField field)
        {
            if (_fields == null)
            {
                _fields = new List<SomeField>();
            }

            this._fields.Add(field);
            field.SomeType = this;
        }

        public void AddMethod(SomeMethod method)
        {
            if (_methods == null)
                _methods = new List<SomeMethod>();

            this._methods.Add(method);
            method.SomeType = this;
        }

        public void AddParentClass(SomeClass parent)
        {
            if (_parentClasses == null)
            {
                _parentClasses = new List<SomeClass>();
            }

            if (!_parentClasses.Contains(parent))
            {
                _parentClasses.Add(parent);
                parent.AddSubClass(this);
            }
        }

        public void AddSubClass(SomeClass sub)
        {
            if (subClasses == null)
            {
                subClasses = new List<SomeClass>();
            }

            if (!subClasses.Contains(sub))
            {
                subClasses.Add(sub);
            }
        }

        public void AddParent(string parent)
        {
            if (_parents == null)
            {
                _parents = new List<string>();
            }
            else
            {
                if (_parents.Count != 0 && this._clsType != ClassType.Interface)
                {
                    SomeErrorReporter.AddError(string.Format("cannot add {0} to {1}'s parents list, only interface is allowed to have multi parents", this.ClassName));
                    return;
                }
            }
            _parents.Add(parent);
        }

        public void AddPropery(SomeProperty prop)
        {
            if (_properties == null)
                _properties = new List<SomeProperty>();

            _properties.Add(prop);
        }

        public bool CheckIsDesendent(string type_ancestor)
        {
            if (this.ClassName == type_ancestor)
            {
                return true;
            }
            else
            {
                if (_parentClasses != null)
                {
                    foreach (SomeClass cls in _parentClasses)
                    {
                        if (cls.CheckIsDesendent(type_ancestor))
                            return true;
                    }
                }
            }

            return false;
        }

        public void BuildRelations(List<SomeClass> classes, bool rebuild)
        {
            //already built?
            if (!rebuild && _parentClasses != null)
            {
                return;
            }

            RebuildRelations(classes);
        }

        public void RebuildRelations(List<SomeClass> classes)
        {
            if (_parents == null)
            {
                //no parents
                return;
            }

            foreach (string str_cls in _parents)
            {
                foreach (SomeClass cls in classes)
                {
                    if (str_cls == cls.ClassName)
                    {
                        this.AddParentClass(cls);
                        break;
                    }
                }
            }
        }

        public bool LookupMemberObjType(string obj_name, ref string type)
        {
            //tyr fields
            if (_fields != null)
            {
                foreach (SomeField fld in _fields)
                {
                    if (fld.Name == obj_name)
                    {
                        type = fld.Type;
                        return true;
                    }
                }
            }
            if (_properties != null)
            {
                foreach (SomeProperty prop in _properties)
                {
                    if (prop.Name == obj_name)
                    {
                        type = prop.Type;
                        return true;
                    }
                }
            }

            //build relations in advance
            this.BuildRelations(SomeCompiler.CodeDom.Classes, false);

            //search from parents
            if (this._parentClasses != null)
            {
                foreach (SomeClass parent in _parentClasses)
                {
                    if (parent.LookupMemberObjType(obj_name, ref type))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public SomeMethod LookupTypeMethod(SomeSequence seq, string method_name)
        {
            foreach (SomeMethod method in this.Methods)
            {
                if (method.Name == method_name)
                {
                    if (seq.CheckParameters(method))
                    {
                        return method;
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            this.BuildRelations(SomeCompiler.CodeDom.Classes, false);

            //search from parents
            if (this._parentClasses != null)
            {
                foreach (SomeClass parent in _parentClasses)
                {
                    SomeMethod method = parent.LookupTypeMethod(seq, method_name);
                    if (method != null)
                    {
                        return method;
                    }
                }
            }

            return null;
        }
    }
}
