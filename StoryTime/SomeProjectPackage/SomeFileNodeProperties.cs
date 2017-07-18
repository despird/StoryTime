using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Project;

namespace SOME.SomeProjectPackage
{
	[ComVisible(true), CLSCompliant(false)]
    [Guid("7142648B-4BC9-4CB3-991B-A7E6DC7D9287")]
	public class SomeFileNodeProperties : SingleFileGeneratorNodeProperties
	{
		#region ctors
		public SomeFileNodeProperties(HierarchyNode node)
			: base(node)
		{
		}
		#endregion

		#region properties
		[Browsable(false)]
		public string URL
		{
			get
			{
				return "file:///" + this.Node.Url;
			}
		}
		[Browsable(false)]
		public string SubType
		{
			get
			{
				return ((SomeFileNode)this.Node).SubType;
			}
			set
			{
                ((SomeFileNode)this.Node).SubType = value;
			}
		}

		[Microsoft.VisualStudio.Project.SRCategoryAttribute(Microsoft.VisualStudio.Project.SR.Advanced)]
		[Microsoft.VisualStudio.Project.LocDisplayName(Microsoft.VisualStudio.Project.SR.BuildAction)]
		[Microsoft.VisualStudio.Project.SRDescriptionAttribute(Microsoft.VisualStudio.Project.SR.BuildActionDescription)]
		public virtual SomeBuildAction SomeBuildAction
		{
			get
			{
				string value = this.Node.ItemNode.ItemName;
				if(value == null || value.Length == 0)
				{
					return SomeBuildAction.None;
				}
                return (SomeBuildAction)Enum.Parse(typeof(SomeBuildAction), value);
			}
			set
			{
				this.Node.ItemNode.ItemName = value.ToString();
			}
		}

		[Browsable(false)]
		public override BuildAction BuildAction
		{
			get
			{
				switch(this.SomeBuildAction)
				{
					case SomeBuildAction.ApplicationDefinition:
					case SomeBuildAction.Page:
					case SomeBuildAction.Resource:
						return BuildAction.Compile;
					default:
						return (BuildAction)Enum.Parse(typeof(BuildAction), this.SomeBuildAction.ToString());
				}
			}
			set
			{
                this.SomeBuildAction = (SomeBuildAction)Enum.Parse(typeof(SomeBuildAction), value.ToString());
			}
		}
		#endregion
	}

	public enum SomeBuildAction { None, Compile, Content, EmbeddedResource, ApplicationDefinition, Page, Resource };
}
