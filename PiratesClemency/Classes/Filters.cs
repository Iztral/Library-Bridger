using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PiratesClemency.Classes
{
    public static class Filters
    {
        public static string Filter_word(string stringtoFilter)
        {
            stringtoFilter = stringtoFilter.Replace("Ft.", "");
            stringtoFilter = stringtoFilter.Replace("ft.", "");
            stringtoFilter = stringtoFilter.Replace("Feat.", "");
            stringtoFilter = stringtoFilter.Replace("feat.", "");
            stringtoFilter = stringtoFilter.Replace("Featuring", "");
            stringtoFilter = stringtoFilter.Replace("featuring", "");
            stringtoFilter = stringtoFilter.Replace("#", "");
            stringtoFilter = stringtoFilter.Replace("Lyrics", "");
            stringtoFilter = stringtoFilter.Replace("lyrics", "");
            string regex = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";
            stringtoFilter = Regex.Replace(stringtoFilter, regex, "");
            return stringtoFilter;
        }

    }
}
