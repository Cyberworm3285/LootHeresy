using System;
using System.Linq;

using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Extensions.Specific;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;

namespace LootHeresyLib.Algorithms
{
    public class PartitionLoot<TKey, TGenerate> : ILootAlgorithm<TKey, TGenerate>
    {
        private Random _rand;
        private int _bias;

        public ILogger Logger { get; set; }

        public PartitionLoot()
        => _rand = new Random();

        public PartitionLoot(Random rand, int bias = 0)
        {
            _rand = rand ?? new Random();
            _bias = bias;
        }

        public ILootable<TKey, TGenerate> Generate(ILootable<TKey, TGenerate>[] arr)
        {
            if (arr.IsNullOrEmpty() || arr.Any(x => x == null))
                Logger.LogAndThrow<ArgumentException>
                (
                    LoggerSeverity.InputValidation,
                    "Array for generation is invalid (null or empty), or including a nil value, causing possible undefined state",
                    arr
                        .Select((x,i) => (x, i))
                        .Where(y => y.x == null)
                        .ToArray()    
                );


            int sum = arr.Sum(x => x.Rarity);

            int index = _rand.Next(_bias, sum);

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
