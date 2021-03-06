﻿using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using SettingsPage=Microsoft.VisualStudio.Project.SettingsPage;
using System.ComponentModel;


//TODOZ: this needs to be reworked after MSBuild target is implemented
namespace SOME.SomeLanguageProject
{
    internal enum GeneralPropertyPageTag
    {
        AssemblyName,        
        RootNamespace,        
        WhiteSpaceAgnostic,
        OutputType,
    }

    [ComVisible(true), Guid(GuidList.guidSomeLanguageProjectPropertyPageClassString)]
    public class GeneralPropertyPage : SettingsPage //, EnvDTE80.IInternalExtenderProvider
    {
        private string assemblyName;
        private OutputType outputType;
        private string defaultNamespace;

        public GeneralPropertyPage()
        {
            this.Name = "General";
        }

        [Category("AssemblyName")]
        [DisplayName("AssemblyName")]
        [Description("The output file holding assembly metadata.")]
        public string AssemblyName
        {
            get { return this.assemblyName; }
        }
        [Category("Application")]
        [DisplayName("OutputType")]
        [Description("The type of application to build.")]
        public OutputType OutputType
        {
            get { return this.outputType; }
            set { this.outputType = value; this.IsDirty = true; }
        }
        [Category("Application")]
        [DisplayName("DefaultNamespace")]
        [Description("Specifies the default namespace for added items.")]
        public string DefaultNamespace
        {
            get { return this.defaultNamespace; }
            set { this.defaultNamespace = value; this.IsDirty = true; }
        }

        protected override void BindProperties()
        {
            this.assemblyName = this.ProjectMgr.GetProjectProperty("AssemblyName", true);
            this.defaultNamespace = this.ProjectMgr.GetProjectProperty("RootNamespace", false);

            string outputType = this.ProjectMgr.GetProjectProperty("OutputType", false);
            this.outputType = (OutputType)Enum.Parse(typeof(OutputType), outputType);
        }

        protected override int ApplyChanges()
        {
            this.ProjectMgr.SetProjectProperty("AssemblyName", this.assemblyName);
            this.ProjectMgr.SetProjectProperty("OutputType", this.outputType.ToString());
            this.ProjectMgr.SetProjectProperty("RootNamespace", this.defaultNamespace);
            this.IsDirty = false;

            return VSConstants.S_OK;
        }
    }
}
