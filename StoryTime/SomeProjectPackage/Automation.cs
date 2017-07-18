using System;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using Microsoft.VisualStudio.Designer.Interfaces;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Samples.VisualStudio.CodeDomCodeModel;

namespace SOME.SomeProjectPackage
{
	/// <summary>
	/// Add support for automation on py files.
	/// </summary>
	[ComVisible(true)]
    [Guid("5D2A8DF9-3831-433E-877A-D01872ADA565")]
	public class OASomeFileItem : OAFileItem
	{
		#region variables
		private EnvDTE.FileCodeModel codeModel;
		#endregion

		#region ctors
		public OASomeFileItem(OAProject project, FileNode node)
			: base(project, node)
		{
		}
		#endregion

		#region overridden methods
		public override EnvDTE.FileCodeModel FileCodeModel
		{
			get
			{
				if(null != codeModel)
				{
					return codeModel;
				}
				if((null == this.Node) || (null == this.Node.OleServiceProvider))
				{
					return null;
				}
				ServiceProvider sp = new ServiceProvider(this.Node.OleServiceProvider);
				IVSMDCodeDomProvider smdProvider = sp.GetService(typeof(SVSMDCodeDomProvider)) as IVSMDCodeDomProvider;
				if(null == smdProvider)
				{
					return null;
				}
				CodeDomProvider provider = smdProvider.CodeDomProvider as CodeDomProvider;
				codeModel = PythonCodeModelFactory.CreateFileCodeModel(this as EnvDTE.ProjectItem, provider, this.Node.Url);
				return codeModel;
			}
		}

		public override EnvDTE.Window Open(string viewKind)
		{
			if(string.Compare(viewKind, EnvDTE.Constants.vsViewKindPrimary) == 0)
			{
				// Get the subtype and decide the viewkind based on the result
				if(((SomeFileNode)this.Node).IsFormSubType)
				{
					return base.Open(EnvDTE.Constants.vsViewKindDesigner);
				}
			}
			return base.Open(viewKind);
		}
		#endregion
	}

	[ComVisible(true)]
	public class OAIronPythonProject : OAProject
	{
		public OAIronPythonProject(SomeProjectNode pythonProject)
			: base(pythonProject)
		{
		}

		public override EnvDTE.CodeModel CodeModel
		{
			get
			{
				return PythonCodeModelFactory.CreateProjectCodeModel(this);
			}
		}
	}

}
