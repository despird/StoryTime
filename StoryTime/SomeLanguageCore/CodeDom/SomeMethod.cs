using System;
using System.Collections.Generic;
using System.Text;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeMethod : SomeTreeNodeMemberBase
    {
        public bool IsConstructor = false;
        
        public bool IsDestructor = false;

        public bool IsEntry = false;

        private SomeClass _someType = null;
        public SomeClass SomeType
        {
            get { return _someType; }
            set { _someType = value; }
        }

        private List<SomeParameter> _parameters;

        private List<SomeStatement> _statements = null;

        public List<SomeParameter> Parameters
        {
            get
            {
                return _parameters;
            }
        }

        public List<SomeStatement> Statements
        {
            get
            {
                return _statements;
            }
            set
            {
                _statements = value;
            }
        }

        public void AddParameter(SomeParameter para)
        {
            if (_parameters == null)
                _parameters = new List<SomeParameter>();

            _parameters.Add(para);
        }

        //public void AddStatement(SomeStatement statment)
        //{
        //    if (this._statements == null)
        //        _statements = new List<SomeStatement>();

        //    _statements.Add(statment);
        //}
    }

    public class SomeParameter : SomeTreeNodeMemberBase
    {
        public string InvokingObj = "";

        public string PassedObj = "";
    }
}
