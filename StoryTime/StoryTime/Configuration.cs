using System;
using System.Collections.Generic;
using System.Text;

namespace StoryTime
{
    /// <summary>
    /// Configuration data from option page
    /// </summary>
    public class Configuration
    {
        #region User Story
        public static List<string> IncludeFileTypes { get; set; }
        #endregion

        //TODOZ: load from Option page
        static Configuration()
        {
            IncludeFileTypes = new List<string>()
            {
                ".cs",
                ".aspx"
            };
        }
    }
}
