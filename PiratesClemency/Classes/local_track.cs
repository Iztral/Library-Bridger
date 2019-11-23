using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiratesClemency.Classes
{
    public enum TagState { TITLE_ONLY, MISSING_TAG, FULL_TAGS }

    public class Local_track
    {
        public string File_name { get; set; }

        public string Author { get; set; }

        public string Title { get; set; }

        public string Error { get; set; }

        public TagState TagState { get; set; }
    }
}
