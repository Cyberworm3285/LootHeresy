using System.Runtime.CompilerServices;

namespace LootHeresyLib.Extensions.Generic
{
    public static class Validation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNull(this object obj)
        => obj == null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this T[] t)
        => t == null || t.Length == 0;
    }
}
