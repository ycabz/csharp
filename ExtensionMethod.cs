using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCabz
{
    internal static class ExtensionMethod
    {
        public static bool EqualIgnoreCase(this string str, string value)
        {
            return str.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool StartsWithIgnoreCase(this string str, string value)
        {
            return str.StartsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithIgnoreCase(this string str, string value)
        {
            return str.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        }

        public static string[] SplitWithoutEmptyEntries(this string str, char separator)
        {
            return str.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
