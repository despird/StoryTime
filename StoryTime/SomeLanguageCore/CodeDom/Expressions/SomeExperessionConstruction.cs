using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOME.SomeLanguageCore.CodeDom
{
    public class SomeExperessionConstruction
    {
        public string ConstructionObject = "";
        public string ConstructionObjectType = "";
        private string constructionObjectRealType = "";

        public string ConstructionObjectRealType
        {
            get
            {
                if (string.IsNullOrEmpty(constructionObjectRealType))
                {
                    return this.ConstructionObjectType;
                }

                return constructionObjectRealType;
            }
            set
            {
                constructionObjectRealType = value;
            }
        }

        public List<SomeParameter> ConstructorParameters = null;

        public void AddParameter(SomeParameter parameter)
        {
            if (this.ConstructorParameters == null)
                this.ConstructorParameters = new List<SomeParameter>();

            this.ConstructorParameters.Add(parameter);
        }
    }

}
