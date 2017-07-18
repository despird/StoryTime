using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Project.Automation;
using SOME.SomeLanguageService;

namespace SOME.SomeProjectPackage
{
    public class SomeReferenceContainerNode : ReferenceContainerNode
    {
        private readonly SomeProjectNode project;

        public SomeReferenceContainerNode(SomeProjectNode root) : base(root)
        {
            this.project = root;
        }

        public override void AddChild(HierarchyNode node)
        {
            base.AddChild(node);

            //IReference reference = null;

            //if (node is AssemblyReferenceNode)
            //{
            //    reference = new AssemblyReference();
            //    reference.Path = ((OAAssemblyReference)node.Object).Path;

            //    project.Sources.References.Add(reference);
            //}
            //else if (node is ProjectReferenceNode)
            //{
            //    reference = new ProjectReference();
            //    reference.Path = ((OAProjectReference)node.Object).Path;

            //    project.Sources.References.Add(reference);
            //}
        }

    }
}