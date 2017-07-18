using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using SOME.SomeLanguageService;
using SOME.SomeLanguageService.Library;
using Microsoft.Windows.Design.Host;
using Microsoft.VisualStudio.Project.Automation;
using VSLangProj;

namespace SOME.SomeProjectPackage
{
  [CLSCompliant(false)]
  [ComVisible(true)]
  [Guid(GuidList.guidSomeProjectClassString)]
    public class SomeProjectNode : ProjectNode, IVsProjectSpecificEditorMap2
  {
    private static ImageList projNoeImageList;
    internal static int fileNodeImageIndex;
    internal static int imageIndex;
    private Microsoft.VisualStudio.Designer.Interfaces.IVSMDCodeDomProvider codeDomProvider;
    private ProjectDocumentsListenerForMainFileUpdates projectDocListenerForMainFileUpdates;

    private ProjectPackage package;
    private VSProject vsProject;
    //private SomeProjectSources projectSources;
    private readonly SomeLanguageService.SomeLanguageService languageService;

    public bool CanDeleteItemsInProject
    {
        get
        {
            return this.CanProjectDeleteItems;
        }
        set
        {
            this.CanProjectDeleteItems = value;
        }
    }

    static SomeProjectNode()
    {
        projNoeImageList =
            Utilities.GetImageList(
            typeof(SomeProjectNode).Assembly.GetManifestResourceStream(
            "SOME.SomeProjectPackage.Resources.SomeProjectNode.bmp"));

        string fileResourceString = "SOME.SomeProjectPackage.Resources.SomeFileNode.bmp";

        ImageList fileNodeImageList =
            Utilities.GetImageList(
            typeof(SomeProjectNode).Assembly.GetManifestResourceStream(
            fileResourceString));

        projNoeImageList.Images.Add(fileNodeImageList.Images[0]);
        projNoeImageList.Images.Add(fileNodeImageList.Images[1]);
    }

    public SomeProjectNode(ProjectPackage package, SomeLanguageService.SomeLanguageService languageService)
    {
      this.package = package;
      this.languageService = languageService;
      imageIndex = this.ImageHandler.ImageList.Images.Count;
      fileNodeImageIndex = imageIndex + 1;

      this.CanDeleteItemsInProject = true;

      foreach (Image img in projNoeImageList.Images)
      {
        this.ImageHandler.AddImage(img);
      }
    }

    /// <summary>
    /// Retreive the CodeDOM provider
    /// </summary>
    protected internal Microsoft.VisualStudio.Designer.Interfaces.IVSMDCodeDomProvider CodeDomProvider
    {
        get
        {
            if (codeDomProvider == null)
                codeDomProvider = new VSMDSomeProvider(this.VSProject);
            return codeDomProvider;
        }
    }
    /// <summary>
    /// Get the VSProject corresponding to this project
    /// </summary>
    protected internal VSLangProj.VSProject VSProject
    {
        get
        {
            if (vsProject == null)
                vsProject = new OAVSProject(this);
            return vsProject;
        }
    }

    internal override object Object
    {
      get
      {
        if (vsProject == null)
          vsProject = new SomeVSProject(this);

        return vsProject;
      }
    }

    public override Guid ProjectGuid
    {
      get { return typeof(SomeProjectFactory).GUID; }
    }

    public override string ProjectType
    {
      get
      {
        //return this.GetType().Name;
        return "SomeProjectType";
      }
    }

    public override void AddFileFromTemplate(string source, string target)
    {
      string ns = this.FileTemplateProcessor.GetFileNamespace(target, this);
      this.FileTemplateProcessor.AddReplace("$nameSpace$", ns);
      this.FileTemplateProcessor.UntokenFile(source, target);
      this.FileTemplateProcessor.Reset();
    }

    public override FileNode CreateFileNode(ProjectElement item)
    {
        if (item == null)
        {
            throw new ArgumentNullException("item");
        }

        string include = item.GetMetadata(ProjectFileConstants.Include);
        SomeFileNode newNode = new SomeFileNode(this, item, fileNodeImageIndex);
        newNode.OleServiceProvider.AddService(typeof(EnvDTE.Project), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
        newNode.OleServiceProvider.AddService(typeof(EnvDTE.ProjectItem), newNode.ServiceCreator, false);
        if (!string.IsNullOrEmpty(include) && Path.GetExtension(include).Equals(".xaml", StringComparison.OrdinalIgnoreCase))
        {
            //Create a DesignerContext for the XAML designer for this file
            newNode.OleServiceProvider.AddService(typeof(DesignerContext), newNode.ServiceCreator, false);
        }
        newNode.OleServiceProvider.AddService(typeof(VSLangProj.VSProject), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
        if (IsCodeFile(include))
        {
            newNode.OleServiceProvider.AddService(
                typeof(SVSMDCodeDomProvider), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
        }

        return newNode;
    }

    /// <summary>
    /// If you don't override this, then all new files you add to your
    /// project will be of type "content" by default and not "compile".
    /// You'll also get AIDS.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public override bool IsCodeFile(string fileName)
    {
        if (new FileInfo(fileName).Extension.ToLower().Contains("smr") || new FileInfo(fileName).Extension.ToLower().Contains("sms"))
        return true;
      else
        return false;
    }

    /// <summary>
    /// Overriding to provide project general property page
    /// </summary>
    /// <returns></returns>
    protected override Guid[] GetConfigurationIndependentPropertyPages()
    {
      Guid[] result = new Guid[1];
      result[0] = typeof(GeneralPropertyPage).GUID;
      return result;
    }

    /// <summary>
    /// provides the guid for configuration dependent settings
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    protected override Guid[] GetConfigurationDependentPropertyPages()
    {
      Guid[] result = new Guid[1];
      result[0] = typeof(SomeBuildPropertyPage).GUID;
      return result;
    }

    public override int ImageIndex
    {

      get { return imageIndex + 0; }

    }

    public override object GetAutomationObject()
    {
      return new SomeOAProject(this);
    }

    public override void Load(string fileName, string location, string name, uint flags, ref Guid iidProject, out int canceled)
    {
      // this needs to be instantiated before the base call, and then start watching
      // the hierarchy after. This is because the base.Load call hits the AddChild
      // method of the references, which add to the project sources. The watching
      // needs to start after the base call because if it's started before, then there
      // aren't any files added yet!

      base.Load(fileName, location, name, flags, ref iidProject, out canceled);

      this.OleServiceProvider.AddService(typeof(SVSMDCodeDomProvider), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
      this.OleServiceProvider.AddService(typeof(System.CodeDom.Compiler.CodeDomProvider), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);

      ISomeLibraryManager libraryManager = Site.GetService(typeof(ISomeLibraryManager)) as ISomeLibraryManager;
      if (null != libraryManager)
      {
          libraryManager.RegisterHierarchy(this.InteropSafeHierarchy);
      }

      //If this is a WPFFlavor-ed project, then add a project-level DesignerContext service to provide
      //event handler generation (EventBindingProvider) for the XAML designer.
      this.OleServiceProvider.AddService(typeof(DesignerContext), new OleServiceProvider.ServiceCreatorCallback(this.CreateServices), false);
    }

    public override int Close()
    {
        if (null != this.projectDocListenerForMainFileUpdates)
        {
            this.projectDocListenerForMainFileUpdates.Dispose();
            this.projectDocListenerForMainFileUpdates = null;
        }

        if (null != Site)
        {
            ISomeLibraryManager libraryManager = Site.GetService(typeof(ISomeLibraryManager)) as ISomeLibraryManager;
            if (null != libraryManager)
            {
                libraryManager.UnregisterHierarchy(this.InteropSafeHierarchy);
            }
        }

        return base.Close();
    }

    private object CreateServices(Type serviceType)
    {
        object service = null;
        if (typeof(SVSMDCodeDomProvider) == serviceType)
        {
            service = this.CodeDomProvider;
        }
        else if (typeof(System.CodeDom.Compiler.CodeDomProvider) == serviceType)
        {
            service = this.CodeDomProvider.CodeDomProvider;
        }
        //else if (typeof(DesignerContext) == serviceType)
        //{
        //    service = this.DesignerContext;
        //}
        else if (typeof(VSLangProj.VSProject) == serviceType)
        {
            service = this.VSProject;
        }
        else if (typeof(EnvDTE.Project) == serviceType)
        {
            service = this.GetAutomationObject();
        }
        return service;
    }

    IVsHierarchy InteropSafeHierarchy
    {
      get
      {
        IntPtr unknownPtr = Utilities.QueryInterfaceIUnknown(this);

        if (unknownPtr == IntPtr.Zero)
          return null;

        return (IVsHierarchy)Marshal.GetObjectForIUnknown(unknownPtr);
      }
    }

    //public SomeProjectSources Sources
    //{
    //  get { return projectSources; }
    //}

    protected override ReferenceContainerNode CreateReferenceContainerNode()
    {
      return new SomeReferenceContainerNode(this);
    }

    public int GetSpecificEditorProperty(string pszMkDocument, int propid, out object pvar)
    {
        throw new NotImplementedException();
    }

    public int GetSpecificEditorType(string pszMkDocument, out Guid pguidEditorType)
    {
        throw new NotImplementedException();
    }

    public int GetSpecificLanguageService(string pszMkDocument, out Guid pguidLanguageService)
    {
        throw new NotImplementedException();
    }

    public int SetSpecificEditorProperty(string pszMkDocument, int propid, object var)
    {
        throw new NotImplementedException();
    }
  }
}
