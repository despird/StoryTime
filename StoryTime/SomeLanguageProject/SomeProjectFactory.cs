using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Project;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace SOME.SomeLanguageProject
{
    [Guid(GuidList.guidSomeLanguageProjectFactoryString)]
    public class SomeProjectFactory: ProjectFactory
    {
        protected override ProjectNode CreateProject()
        {
            SomeProjectNode project = new SomeProjectNode(this.package);

            project.SetSite((IOleServiceProvider)((IServiceProvider)this.package).GetService(typeof(IOleServiceProvider)));
            return project;
        }

        private SomeLanguageProjectPackage package;
        public SomeProjectFactory(SomeLanguageProjectPackage package)
            : base(package)
        {
            this.package = package;
        }


    }
}
