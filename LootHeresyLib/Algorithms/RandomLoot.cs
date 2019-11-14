using System;
using System.Linq;

using LootHeresyLib.Loot;
using LootHeresyLib.Logger;
using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Extensions.Specific;

namespace LootHeresyLib.Algorithms
{
    public class RandomLoot<TKey, TGenerate> : ILootAlgorithm<TKey, TGenerate>
    {
        private Random _random;

        public ILogger Logger { get; set; }

        public RandomLoot()
        => _random = new Random();

        public RandomLoot(Random rand)
        => _random = rand;

        public ILootable<TKey, TGenerate> Generate(ILootable<TKey, TGenerate>[] arr)
        {
            if (arr.IsNullOrEmpty())
            {
                Logger?.LogOrThrow<ArgumentException>(LoggerSeverity.InputValidation | LoggerSeverity.Error, "Array for generation is invalid (null or empty), causing possible undefined state", this);
                return null;
            }

            int i = _random.Next(arr.Length);
            Logger?.Log("select pseudorandom x :" + Environment.NewLine + string.Join(Environment.NewLine, arr.Select(x => $"\t[{x.Key}]")) + Environment.NewLine, LoggerSeverity.Info);
            Logger?.Log($"result: {arr[i]}", LoggerSeverity.Info | LoggerSeverity.Result);
            return arr[i];
        }
    }
}
