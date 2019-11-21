using LootHeresyLib.Loot;
using LootHeresyLib.Logger;

namespace LootHeresyLib.Algorithms
{
    public interface ILootAlgorithm<TKey, TGenerate>
    {
        ILogger Logger { get; set; }
        ILootable<TKey, TGenerate> Generate(ILootable<TKey, TGenerate>[] arr);
    }
}
