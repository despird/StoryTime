using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.Win32;
using EnvDTE;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace SOME.SomeProjectPackage
{
    [ComVisible(true), CLSCompliant(false), System.Runtime.InteropServices.ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid(GuidList.guidSomeProjectNodePropertiesClassString)]
    public class SomeProjectNodeProperties : ProjectNodeProperties
    {
        public SomeProjectNodeProperties(ProjectNode node)
            : base(node)
        {
        }
    }
}
