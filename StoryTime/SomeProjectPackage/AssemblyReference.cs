using System.Reflection;
using SOME.SomeLanguageService;

namespace SOME.SomeProjectPackage
{
    internal class AssemblyReference : IReference
    {
        public Assembly GetAssembly()
        {
            return AssemblyHelper.FindInCurrentAppDomainOrLoad(Path);
        }

        public string Path { get; set; }
    }
}