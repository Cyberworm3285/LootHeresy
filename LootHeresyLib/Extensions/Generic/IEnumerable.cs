using System;
using System.Collections.Generic;
using System.Linq;

namespace LootHeresyLib.Extensions.Generic
{
    public static class IEnumerbleExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> en, Action<T> a)
        {
            foreach (var x in en)
                a(x);
        }

        public static IEnumerable<T> ForEachForward<T>(this IEnumerable<T> en, Action<T> a)
        {
            foreach (var x in en)
            {
                a(x);
                yield return x;
            }
        }

        public static string Format<T>(this IEnumerable<T> en, Func<T, string> f)
        => $"[{string.Join(",", en.Select(f))}]";

        public static string Format<T>(this IEnumerable<T> en)
        => $"[{string.Join(",", en)}]";
    }
}
