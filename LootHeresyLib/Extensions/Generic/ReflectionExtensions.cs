using System;
using System.Linq;

namespace LootHeresyLib.Extensions.Generic
{
    public static class ReflectionExtensions
    {
        public static TCast Initialize<TCast>(this Type t, params object[] par)
        {
            var ctor = t.GetConstructor(par.Select(x => x.GetType()).ToArray());
            if (ctor.IsNull())
                throw new ArgumentException("Type does not contain matching constructor");

            return (TCast)ctor.Invoke(par);
        }

        public static TCast Initialize<TCast>(this Type t)
            where TCast : new()
        {
            var dummy = new Type[0];
            return (TCast)t.GetConstructor(dummy).Invoke(dummy);
        }
    }
}
