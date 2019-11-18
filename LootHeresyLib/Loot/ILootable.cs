using System.Collections.Generic;

namespace LootHeresyLib.Loot
{
    public interface ILootable<TKey, TGenerate>
    {
        int Rarity { get; }
        TKey Key { get; }

        TGenerate Generate(Stack<TKey> generationStack);

        /// <summary>returns true if still avaiable after method call</summary>
        bool UpdateAvailability();
    }
}
