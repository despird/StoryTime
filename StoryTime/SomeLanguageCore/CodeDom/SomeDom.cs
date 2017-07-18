using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeDom : SomeTreeNodeBase
    {
        //public SomeDom()
        //{
        //    this._name = name.Replace(" ","_");
        //}

        //private string _name;

        //public string Name
        //{
        //    get { return _name; }
        //    set { _name = value; }
        //}


        private List<SomeClass> _classes;

        public List<SomeClass> Classes
        {
            get
            {
                if (_classes == null)
                {
                    _classes = new List<SomeClass>();
                }
                return _classes;
            }
        }

        private List<SomeSequence> _sequences;

        public void AddClass(SomeClass cls)
        {
            if (_classes == null)
                _classes = new List<SomeClass>();

            //prevent duplicate type inserted, ignore the second insertion (TODO: log a warning in the reporter)
            if (cls != null/* && !_classes.Any(c=>c.ClassName == cls.ClassName)*/)
            {
                _classes.RemoveAll(c => c.ClassName == cls.ClassName);
                _classes.Add(cls);
            }
        }

        public void AddSequence(SomeSequence seq)
        {
            if (_sequences == null)
                _sequences = new List<SomeSequence>();

            if (seq != null)
            {
                _sequences.Add(seq);
            }
        }

        //parse the methods from sequences into classes
        public void Parse()
        {
            BuildRelations(false);
            ParseMethods();
        }

        public void BuildRelations(bool rebuild)
        {
            //build type relations
            if (_classes != null)
            {
                foreach (SomeClass cls in _classes)
                {
                    cls.BuildRelations(_classes, rebuild);
                }
            }
        }

        public void ParseMethods()
        {
            if (_sequences != null)
            {
                foreach (SomeSequence seq in _sequences)
                {
                    seq.ParseStatements(_classes);
                }
            }
        }

        //check the constraint
        public void Check()
        {
        }

        //Adds a statement into current open method
        public void AddStatement(SomeStatement statement)
        {
            //TODOZ:
        }

        //Adds a statement into current open method as a method call
        public void AddMethodCall(SomeStatement statement)
        {
            //TODOZ:
        }

        public string[] GetNameSpaces()
        {
            var result = from cls in _classes select cls.NameSpace;
            return result.Distinct().ToArray();
        }

        public void ClearSequences()
        {
            if (_sequences != null)
            {
                _sequences.Clear();
            }
        }
    }
}
