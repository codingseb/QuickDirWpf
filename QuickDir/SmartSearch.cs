using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace QuickDir
{
    internal class SmartSearch
    {
        private static Regex regexSpecialCharsEscape = new Regex(@"[\[\](){}.+?*]");

        public static string GetRegexSmartSearchPattern(string find)
        {
            return ".*" + Regex.Replace(find, ".", delegate(Match match)
            {
                return regexSpecialCharsEscape.Replace(match.Value, delegate(Match sCharMatch)
                {
                    return @"\" + sCharMatch.Value;
                })
                    
                + ".*";
            });
        }

        public static List<string> SmartSearchInDirectories(string find)
        {
            string findPattern = GetRegexSmartSearchPattern(find);

            List<string> allDirectories = new List<string>();

            

            return allDirectories;
        }
    }
}
