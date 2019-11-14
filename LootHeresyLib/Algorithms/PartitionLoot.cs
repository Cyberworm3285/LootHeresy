using System;
using System.Linq;

using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;

namespace LootHeresyLib.Algorithms
{
    public class PartitionLoot<TKey, TGenerate> : ILootAlgorithm<TKey, TGenerate>
    {
        private Random _rand;

        public ILogger Logger { get; set; }

        public PartitionLoot()
        => _rand = new Random();

        public PartitionLoot(Random rand)
        => _rand = rand;

        public ILootable<TKey, TGenerate> Generate(ILootable<TKey, TGenerate>[] arr)
        {
            if (arr.IsNullOrEmpty())
            {
                Logger?.Log("Array for generation is invalid (null or empty), causing possible undefined state", LoggerSeverity.InputValidation | LoggerSeverity.Error);
                return null;
            }


            int sum = arr.Sum(x => x.Rarity);

            int index = _rand.Next(sum);

            if (Logger != null)
            {
                int partitions = 0;
                Logger.Log(
                    $"picking x :"
                +   Environment.NewLine
                +   string.Join(Environment.NewLine, arr.Select
                    (
                        x =>
                        {
                            string res = $"\t[{x.Rarity}(start : {partitions}) : {x.Key}]";
                            partitions += x.Rarity;
                            return res;
                        })
                    )
                +   $"{Environment.NewLine} with value \"{index}\"..", LoggerSeverity.Info | LoggerSeverity.Result);
            }

            int counter = 0;
            int curr = 0;
            while (curr + arr[counter].Rarity <= index)
            {
                curr += arr[counter].Rarity;
                counter++;
            }

            Logger?.Log($"Result: {arr[counter]}", LoggerSeverity.Info);

            return arr[counter];
        }
    }
}
