using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Project;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Windows.Design.Host;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VSConstants = Microsoft.VisualStudio.VSConstants;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio;
using EnvDTE;
using EnvDTE80;
using SOME.SomeLanguageCore;
using Microsoft.VisualStudio.Project.Automation;
using SOME.SomeLanguageGenerator;
using SOME.SomeLanguageCore.Tokens;


namespace SOME.SomeLanguageProject
{
    public class SomeProjectNode : ProjectNode
    {
        private SomeLanguageProjectPackage package;

        private static ImageList projNoeImageList;
        internal static int fileNodeImageIndex;
        internal static int imageIndex;

        public override int ImageIndex
        {
            get { return imageIndex; }
        }

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
                "SOME.SomeLanguageProject.Resources.SomeProjectNode.bmp"));

            string fileResourceString = "SOME.SomeLanguageProject.Resources.SomeFileNode.bmp";

            ImageList fileNodeImageList =
                Utilities.GetImageList(
                typeof(SomeProjectNode).Assembly.GetManifestResourceStream(
                fileResourceString));

            projNoeImageList.Images.Add(fileNodeImageList.Images[0]);
            projNoeImageList.Images.Add(fileNodeImageList.Images[1]);
        }

        public SomeProjectNode(SomeLanguageProjectPackage package)
        {
            this.package = package;
            imageIndex = this.ImageHandler.ImageList.Images.Count;
            fileNodeImageIndex = imageIndex + 1;

            this.CanDeleteItemsInProject = true;

            foreach (Image img in projNoeImageList.Images)
            {
                this.ImageHandler.AddImage(img);
            }
        }

        public override FileNode CreateFileNode(ProjectElement item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            string include = item.GetMetadata(ProjectFileConstants.Include);
            SomeFileNode newNode = new SomeFileNode(this, item, fileNodeImageIndex);

            //Load all types into memory for Editor's syntax colouring
            string fileName = item.GetFullPathForElement();
            if (fileName.EndsWith(SomeLanguageService.SomeLanguageService.LANGUAGE_EXT1))
            {
                string codeName = Path.GetFileNameWithoutExtension(fileName);
                string text = File.ReadAllText(fileName);
                SomeCompiler.ParseTypes(text, codeName);
            }
            return newNode;
        }

        public override void AddFileFromTemplate(string source, string target)
        {
            string nameSpace = this.FileTemplateProcessor.GetFileNamespace(target, this);
            //string className = Path.GetFileNameWithoutExtension(target);

            this.FileTemplateProcessor.AddReplace("$nameSpace$", nameSpace);
            //this.FileTemplateProcessor.AddReplace("$className$", className);

            this.FileTemplateProcessor.UntokenFile(source, target);
            this.FileTemplateProcessor.Reset();
        }

        public override Guid ProjectGuid
        {
            get { return GuidList.guidSomeLanguageProjectFactory; }
        }
        public override string ProjectType
        {
            get { return "SomeProjectType"; }
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
            result[0] = typeof(GeneralPropertyPage).GUID;
            return result;
        }

        public override int ExecCommand(uint itemId, ref Guid guidCmdGroup, uint nCmdId, uint nCmdExecOpt, IntPtr pvain, IntPtr p)
        {
            /*
             * using the following way to intercept a menu execution
                if (guidCmdGroup.CompareTo(new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}")) == 0 && nCmdId == 356)
             * 
             * ref: http://social.msdn.microsoft.com/forums/en-US/vsx/thread/3d60c095-0bcc-43cd-9d2a-7c383760e2d1 regarding 
             * how to get a guid and cmdId for a menu item
             */

            //intercepting Build command
            if (guidCmdGroup == Microsoft.VisualStudio.Shell.VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)nCmdId)
                {
                    case VsCommands.BuildCtx:
                        {
                            BuildTask();
                        }
                        return VSConstants.S_OK;
                    case VsCommands.CleanCtx:
                        {
                            CleanTask();
                        }
                        return VSConstants.S_OK;
                }
            }

            if (guidCmdGroup == GuidList.guidSomeLanguageProjectCmdSet)
            {
                switch (nCmdId)
                {
                    case PkgCmdIDList.cmdidCompile:
                        {
                            BuildTask();
                        } return VSConstants.S_OK;
                    case PkgCmdIDList.cmdidGenerateCSharp:
                        {
                            Generate(SomeLanguage.CSharp);
                        }
                        return VSConstants.S_OK;
                    case PkgCmdIDList.cmdidGenerateVB:
                        {
                            Generate(SomeLanguage.VBNet);
                        }
                        return VSConstants.S_OK;
                }
            }

            return base.ExecCommand(itemId, ref guidCmdGroup, nCmdId, nCmdExecOpt, pvain, p);
        }

        protected void Generate(SomeLanguage lang)
        {
            //http://msdn.microsoft.com/en-us/library/envdte.project.save%28v=vs.100%29.aspx

            if (!BuildTask())
                return;

            DTE2 dte = (DTE2)GetService(typeof(DTE));
            Solution2 sln = dte.Solution as Solution2;

            SomeGenerator generator = SomeGenerator.GetConcreteGenerator(lang, SomeCompiler.CodeDom);

            IEnumerable<string> projectNames = generator.GetGenerateProjectNames();
            foreach (string projectName in projectNames)
            {
                Project projNew = CreateProjectInSolution(sln, projectName, generator);
                string projDir = Path.GetDirectoryName(projNew.FullName);
                IEnumerable<string> srcFiles = generator.GenerateProjectSource(projDir, projectName);
                foreach (string src in srcFiles)
                {
                    projNew.ProjectItems.AddFromFile(src);
                    projNew.Save(projNew.FullName);
                }
            }
            sln.SaveAs(sln.FullName);
        }

        private Project CreateProjectInSolution(Solution2 sln, string projName, SomeGenerator generator)
        {
            string[] templatePair = generator.GetLangugeTemplate();
            var templatePath = sln.GetProjectTemplate(templatePair[0], templatePair[1]);
            //http://msdn.microsoft.com/en-us/library/envdte._solution.addfromtemplate.aspx
            //AddFromTemplate(); For Visual Basic and Visual C# projects: The Project object returned is null. 
            //You can find the Project object created by iterating through the DTE.Solution.Projects 
            //collection using the ProjectName parameter to identify the newly created project.
            string projDir = string.Format(@"{0}\{1}", Path.GetDirectoryName(sln.FullName), projName);
            sln.AddFromTemplate(templatePath, projDir, projName);
            Project projNew = null;
            foreach (Project proj in sln.Projects)
            {
                if (proj.Name == projName)
                {
                    projNew = proj;
                    break;
                }
            }

            foreach (ProjectItem item in projNew.ProjectItems)
            {
                if (item.Name == "Class1" + generator.GetSourceFileExtension())
                {
                    item.Remove();
                    File.Delete(Path.Combine(projDir, item.Name));
                    break;
                }
            }
            return projNew;
        }

        Guid paneGuid = new Guid();

        public void GetSourceFiles(ProjectItems items, List<string> files)
        {
            foreach (ProjectItem item in items)
            {
                if (item.Kind.ToUpper() == EnvDTE.Constants.vsProjectItemKindPhysicalFile.ToUpper())
                {
                    files.Add(item.FileNames[1]);
                }
                else if (item.Kind.ToUpper() == EnvDTE.Constants.vsProjectItemKindPhysicalFolder.ToUpper())
                {
                    GetSourceFiles(item.ProjectItems, files);
                }
            }
        }

        protected bool CleanTask()
        {
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            Solution2 sln = dte.Solution as Solution2;

            for (int i = sln.Projects.Count; i > 0; i--)
            {
                Project proj = sln.Projects.Item(i);
                if (proj.FullName != (this.GetAutomationObject() as Project).FullName)
                {
                    string projDir = Path.GetDirectoryName(proj.FullName);
                    sln.Remove(proj);

                    int tryCount = 0;
                    while (tryCount < 10)
                    {
                        try
                        {
                            Directory.Delete(projDir, true);
                            tryCount = 10;
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(100);
                            tryCount++;
                        }
                    }
                }
            }
            sln.SaveAs(sln.FullName);
            return true;
        }

        protected bool BuildTask()
        {
            DTE2 dte = (DTE2)GetService(typeof(DTE));
            EnvDTE.Project proj = this.GetAutomationObject() as Project;

            //Save all unsaved project items
            proj.Save("");

            //Save the project
            this.BuildProject.Save(); //or proj.Save(proj.FullName);

            if (paneGuid != new Guid())
            {
                IVsOutputWindow output = (IVsOutputWindow)GetService(typeof(SVsOutputWindow));
                output.DeletePane(ref paneGuid);
            }

            OutputWindow ow = dte.ToolWindows.OutputWindow;
            OutputWindowPane owP = ow.OutputWindowPanes.Add("Build");
            paneGuid = new Guid(owP.Guid);

            List<string> files = new List<string>();
            GetSourceFiles(proj.ProjectItems, files);

            files.Sort((x, y) =>
            {
                if (x.EndsWith(".smr") && y.EndsWith(".sms"))
                    return -1;
                if (x.EndsWith(".sms") && y.EndsWith(".smr"))
                    return 1;
                else
                    return 0;
            });

            owP.Clear();

            SomeCompiler.Compile(files.ToArray());
            if (SomeErrorReporter.HasErrors)
            {
                SomeErrorReporter.GetReports().ForEach(report =>
                {
                    owP.OutputTaskItemString(
                        report.Message,
                        vsTaskPriority.vsTaskPriorityHigh,
                        vsTaskCategories.vsTaskCategoryBuildCompile,
                        vsTaskIcon.vsTaskIconComment,
                        report.File,
                        report.LineNumber,
                        report.Message,
                        true);
                });

                owP.ForceItemsToTaskList();
                dte.StatusBar.Text = "Build failed";
                return false;
            }
            else
            {
                owP.OutputString("Build succeeded." + Environment.NewLine);
                dte.StatusBar.Text = "Build succeeded.";
            }

            return true;

        }

        //Hide/Show menu items for project node
        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText, ref QueryStatusResult result)
        {
            /*
             * using the following way to intercept a menu execution
                if (guidCmdGroup.CompareTo(new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}")) == 0 && nCmdId == 356)
             * 
             * ref: http://social.msdn.microsoft.com/forums/en-US/vsx/thread/3d60c095-0bcc-43cd-9d2a-7c383760e2d1 regarding 
             * how to get a guid and cmdId for a menu item
             */

            string cmdText = Marshal.PtrToStringAuto(pCmdText);

            //Hiding Debug -> Start new instance, Step Into
            if (cmdGroup.CompareTo(new Guid("{1496A755-94DE-11D0-8C3F-00C04FC2AAE2}")) == 0 && (cmd == 356 || cmd == 357))
            {
                result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                return VSConstants.S_OK;
            }
            //Hiding "Rebuild"
            else if (cmdGroup == Microsoft.VisualStudio.Shell.VsMenus.guidStandardCommandSet97)
            {
                switch ((VsCommands)cmd)
                {
                    case VsCommands.RebuildCtx:
                        //case VsCommands.CleanCtx:
                        result = QueryStatusResult.SUPPORTED | QueryStatusResult.INVISIBLE;
                        return VSConstants.S_OK;
                }
            }
            else if (cmdGroup == GuidList.guidSomeLanguageProjectCmdSet)
            {
                switch (cmd)
                {
                    case PkgCmdIDList.cmdidGenerate:
                    case PkgCmdIDList.cmdidGenerateSubMenuGroup:
                    case PkgCmdIDList.cmdidGenerateCSharp:
                    case PkgCmdIDList.cmdidGenerateVB:
                        result |= QueryStatusResult.SUPPORTED | QueryStatusResult.ENABLED;
                        return VSConstants.S_OK;
                }
            }

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

    }
}