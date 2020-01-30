using System;
using System.Text.RegularExpressions;

namespace LibraryBridger.Generic
{
    public static class Filters
    {
        private static string ReplaceCaseInsensitive(string input, string search, 
            string replacement)
        {
            string result = Regex.Replace(
                input,
                Regex.Escape(search),
                replacement.Replace("$", "$$"),
                RegexOptions.IgnoreCase
            );
            return result;
        }

        private static bool Contains(this string source, string toCheck,
            StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        public static string Filter_word(string stringtoFilter)
        {
            if(stringtoFilter == null)
            {
                return null;
            }
            else
            {
                stringtoFilter = ReplaceCaseInsensitive(stringtoFilter, "ft.", "");
                stringtoFilter = ReplaceCaseInsensitive(stringtoFilter, "feat.", "");
                stringtoFilter = ReplaceCaseInsensitive(stringtoFilter, "featuring", "");
                stringtoFilter = ReplaceCaseInsensitive(stringtoFilter, "#", "");
                stringtoFilter = ReplaceCaseInsensitive(stringtoFilter, "lyrics", "");

                if (!(stringtoFilter.Contains("remix", StringComparison.OrdinalIgnoreCase)))
                {
                    string regex = "(\\[.*\\])|(\\(.*\\))";
                    stringtoFilter = Regex.Replace(stringtoFilter, regex, "");
                }
                return stringtoFilter;
            }
        }

    }
}
