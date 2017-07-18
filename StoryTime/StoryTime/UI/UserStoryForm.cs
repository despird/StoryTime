using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using EnvDTE;
using EnvDTE80;
using EnvDTE90;
using EnvDTE100;
using VSLangProj;
using System.Runtime.InteropServices;
using System.IO;
using StoryTime.Model;
using System.Text.RegularExpressions;

namespace StoryTime.UI
{
    public partial class UserStoryForm : UserControl
    {
        public DTE2 _dte { get; set; }

        public const string GUID = "{28F77869-B2AC-4EB5-A097-C92D62FA64D8}";

        public UserStoryForm()
        {
            InitializeComponent();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshTags();
        }

        private void RefreshTags()
        {
            Solution sln = _dte.Solution;

            List<StoryTag> tags = new List<StoryTag>();

            //Iterate through all files in the solution and find all tags
            foreach (Project proj in sln.Projects)
            {
                foreach (ProjectItem item in proj.ProjectItems)
                {
                    if (item.Object is VSProjectItem)
                    {
                        VSProjectItem file = item.Object as VSProjectItem;
                        
                        //Check if the file is in type list
                        string extension = Path.GetExtension(file.ProjectItem.Name);
                        if(Configuration.IncludeFileTypes.Contains(extension))
                        {
                            BuildTagsFromDocument(item, tags);
                        }
                    }
                }
            }

            tvStoryTag.Nodes.Clear();

            foreach (var tag in tags)
            {
                TreeNode node = new TreeNode(tag.StoryLine);
                node.Tag = tag;
                tvStoryTag.Nodes.Add(node);
            }

            //Order story items by tag numbering
        }

        private void BuildTagsFromDocument(ProjectItem item, List<StoryTag> tags)
        {
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                _dte.MainWindow.Activate();
                string filePath = item.FileNames[0];

                string text;

                if (item.IsOpen)
                {
                    //TextDocument textDoc = (TextDocument)(_dte.ActiveDocument.Object("TextDocument"));
                    TextDocument textDoc = (TextDocument)(item.Document.Object("TextDocument"));
                    text = textDoc.StartPoint.CreateEditPoint().GetText(textDoc.EndPoint);
                }
                else
                {
                    text = File.ReadAllText(filePath);
                }

                Regex regex = new Regex(StoryTag.TagPattern);

                string[] lines = Regex.Split(text, "\r\n|\r|\n");

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    MatchCollection matches = regex.Matches(line);
                    foreach (Match mt in matches)
                    {
                        StoryTag tag = new StoryTag
                        {
                            FileItem = filePath,
                            TagIndex = ParseTagIndex(mt.Value),
                            StoryLine = ParseStoryLine(mt.Value),
                            TagLineNo = i + 1
                        };
                        tags.Add(tag);
                    }
                }
            }
            finally
            {
                Cursor.Current = Cursors.Arrow;
            }
        }

        private string ParseTagIndex(string storyTag)
        {
            return Regex.Match(storyTag, @"[\d.]+").Value;
        }

        private string ParseStoryLine(string storyTag)
        {
            return Regex.Match(storyTag, @"\('.*'\)").Value.Replace("('", "").Replace("')", "");
        }

        private void tvStoryTag_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var node = e.Node;
            var tag = node.Tag as StoryTag;
            _dte.ItemOperations.OpenFile(tag.FileItem, EnvDTE.Constants.vsViewKindTextView);
            TextSelection sel = (TextSelection)(_dte.ActiveDocument.Selection);
            sel.GotoLine(tag.TagLineNo);
            sel.SelectLine();
        }
    }
}
