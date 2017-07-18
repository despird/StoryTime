using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SOME.SomeLanguageCore.Tokens
{
    public class SomeBasicTypes
    {
        //.NET common built-in value types
        //http://msdn.microsoft.com/en-us/library/hfa3fa08(v=vs.80).aspx

        public const string Object = "object";
        public const string Int = "int";
        public const string Short = "short";
        public const string Long = "long";
        public const string Byte = "byte";
        public const string Float = "float";
        public const string Double = "double";
        public const string Char = "char";
        public const string Decimal = "decimal";
        public const string String = "string";
        public const string Bool = "bool";

        public static string TyeGetBasicTypeName(string typeName)
        {
            switch (typeName)
            {
                case SomeBasicTypes.String:
                    return "System.String";
                case SomeBasicTypes.Long:
                    return "System.Int64";
                case SomeBasicTypes.Int:
                    return "System.Int32";
                case SomeBasicTypes.Short:
                    return "System.Int16";
                case SomeBasicTypes.Byte:
                    return "System.Byte";
                case SomeBasicTypes.Float:
                    return "System.Single";
                case SomeBasicTypes.Double:
                    return "System.Double";
                case SomeBasicTypes.Bool:
                    return "System.Boolean";
                case SomeBasicTypes.Char:
                    return "System.Char";
                case SomeBasicTypes.Object:
                    return "System.Object";
                default:
                    return typeName;
            }
        }

        public static bool TryParseBasicType(string typeName, string objName)
        {
            switch (typeName)
            {
                case SomeBasicTypes.Int:
                case SomeBasicTypes.Long:
                case SomeBasicTypes.Byte:
                case SomeBasicTypes.Short:
                    long try_int;
                    return long.TryParse(objName, out try_int);
                case SomeBasicTypes.Float:
                case SomeBasicTypes.Double:
                    double try_double;
                    return double.TryParse(objName, out try_double);
                case SomeBasicTypes.Char:
                    return (objName.Length == 3 && objName[0] == '\'' && objName[objName.Length - 1] == '\'');
                case SomeBasicTypes.String:
                    return (objName.Length >= 2 && objName[0] == '"' && objName[objName.Length - 1] == '"');
                case SomeBasicTypes.Decimal:
                    decimal try_decimal;
                    return decimal.TryParse(objName, out try_decimal);
                case SomeBasicTypes.Bool:
                    bool try_bool;
                    return bool.TryParse(objName, out try_bool);
                case SomeBasicTypes.Object:
                    return true;
            }
            return false;
        }

        public static bool IsBasicType(string typeName)
        {
            return BasicTypeNames.Contains(typeName);
        }

        private static List<string> _basicTypeNames;
        public static List<string> BasicTypeNames
        {
            get
            {
                if (_basicTypeNames == null)
                {
                    _basicTypeNames = new List<string>();
                    Type type = typeof(SomeBasicTypes);
                    SomeBasicTypes bt = new SomeBasicTypes();
                    FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
                    fields.ToList().ForEach(fd => _basicTypeNames.Add((string)fd.GetValue(bt)));
                }
                return _basicTypeNames;
            }
        }

    }
}
