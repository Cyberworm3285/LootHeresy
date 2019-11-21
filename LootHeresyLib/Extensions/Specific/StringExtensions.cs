using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Extensions.Specific
{
    public static class StringExtensions
    {
        public static string Surround(this string s, string pattern)
        => string.IsNullOrEmpty(s)
            ? ""
            : string.Format(pattern, s);
    }
}
