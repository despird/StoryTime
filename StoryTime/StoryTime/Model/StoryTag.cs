using System;
using System.Collections.Generic;
using System.Text;

namespace StoryTime.Model
{
    public class StoryTag: IComparable
    {
        public const string TagPattern = @"{{STU-[\d.]+\('[^']*'\)}}";

        public string FileItem { get; set; }

        public string TagIndex { get; set; }
        
        public int TagLineNo { get; set; }

        public string StoryLine { get; set; }

        public StoryTag Parent { get; set; }

        public static void BuildOutliningTagTree(List<StoryTag> tags)
        {

        }

        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
