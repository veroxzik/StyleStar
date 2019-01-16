using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StyleStar
{
    public static class StringExtensions
    {
        public static string ParseString(string input, string tag)
        {
            return input.Split(new string[] { tag }, StringSplitOptions.RemoveEmptyEntries)[0].Replace("\"", "");
        }

        public static bool TrySearchTag(string input, string tag, out string output)
        {
            output = "";
            if (Regex.IsMatch(input, "(#" + tag + " )"))
            {
                output = ParseString(input, "#" + tag + " ");
                return true;
            }
            else
                return false;
        }
    }
}
