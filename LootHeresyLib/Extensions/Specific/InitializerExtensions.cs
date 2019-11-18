using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Extensions.Specific
{
    internal static class InitializerExtensions
    {
        internal static void Add<T>(this Stack<T> s, T t)
        => s.Push(t);

        internal static void Add<T>(this Queue<T> q, T t)
        => q.Enqueue(t);
    }
}
