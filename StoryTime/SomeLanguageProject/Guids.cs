// Guids.cs
// MUST match guids.h
using System;

namespace SOME.SomeLanguageProject
{
    static class GuidList
    {
        public const string guidSomeLanguageProjectPkgString = "dafcfafb-73a2-46b9-a0e9-84ce5b898127";
        public const string guidSomeLanguageProjectCmdSetString = "103c22c5-a3bf-469e-bc5a-ca5f1a6df0f4";
        public const string guidSomeLanguageProjectFactoryString = "8F66E85B-1CA6-40A8-9817-328FCB770139";
        public const string guidSomeLanguageProjectPropertyPageClassString = "E13BD1B5-9826-45F8-969E-5CC8B52AB0F1";

        public static readonly Guid guidSomeLanguageProjectCmdSet = new Guid(guidSomeLanguageProjectCmdSetString);
        public static readonly Guid guidSomeLanguageProjectFactory = new Guid(guidSomeLanguageProjectFactoryString);
    };
}