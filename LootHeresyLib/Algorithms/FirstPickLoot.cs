using System;
using System.Collections.Generic;
using System.Text;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;

namespace LootHeresyLib.Algorithms
{
    public class FirstPickLoot<TKey, TGenerate> : ILootAlgorithm<TKey, TGenerate>
    {
        public ILogger Logger { get; set; }

        public ILootable<TKey, TGenerate> Generate(ILootable<TKey, TGenerate>[] arr)
        => arr[0];
    }
}
