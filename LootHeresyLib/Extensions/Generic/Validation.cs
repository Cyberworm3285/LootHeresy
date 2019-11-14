using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LootHeresyLib.Extensions.Generic
{
    public static class Validation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull<T>(this T t)
            where T : class
        => t == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this T[] t)
            where T : class        
        => t == null || t.Length == 0;
    }
}
