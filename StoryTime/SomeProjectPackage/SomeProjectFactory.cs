using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.Build.BuildEngine;
using System.Configuration;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using SOME.SomeLanguageService;

namespace SOME.SomeProjectPackage
{
    [Guid(GuidList.guidSomeLangProjectFactoryClassString)]
    [ComVisible(true)]
    public class SomeProjectFactory : ProjectFactory
    {
        private ProjectPackage package;
        public SomeProjectFactory(Package package)
            : base(package)
        {
            this.package = (ProjectPackage)package;
        }

        protected override ProjectNode CreateProject()
        {
            var provider = (IServiceProvider)Package;

            //SomeProjectNode project = new SomeProjectNode(package, (SomeLanguageService.SomeLanguageService)provider.GetService(typeof(SomeLanguageService.SomeLanguageService)));

            //project.SetSite((IOleServiceProvider)provider.GetService(typeof(IOleServiceProvider)));

            //return project;

            SomeProjectNode project = new SomeProjectNode(package, (SomeLanguageService.SomeLanguageService)provider.GetService(typeof(SomeLanguageService.SomeLanguageService)));
            project.SetSite((IOleServiceProvider)((IServiceProvider)this.Package).GetService(typeof(IOleServiceProvider)));
            return project;

        }

        //protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        //{
        //    object ret = base.PreCreateForOuter(outerProjectIUnknown);

        //    return ret;
        //}

        //protected override string ProjectTypeGuids(string file)
        //{
        //    return base.ProjectTypeGuids(file);
        //}
    }
}