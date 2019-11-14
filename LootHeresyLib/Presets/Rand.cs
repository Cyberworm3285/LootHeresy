using System;


namespace LootHeresyLib.Presets
{
    public static class Rand
    {
        private static Random _rand;

        public static int Next(int a) => _rand.Next(a);
        public static int Next(int a, int b) => _rand.Next(a, b);
        public static Random GetRandom() => _rand ?? (_rand = new Random());
    }
}
