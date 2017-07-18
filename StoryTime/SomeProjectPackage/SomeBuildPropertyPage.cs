using System;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Package;
using Microsoft.Win32;

namespace SOME.SomeProjectPackage
{
    [ComVisible(true), Guid(GuidList.guidSomeProjectBuildPropertyPageClassString)]
    public class SomeBuildPropertyPage : BuildPropertyPage
    {

    }
}
