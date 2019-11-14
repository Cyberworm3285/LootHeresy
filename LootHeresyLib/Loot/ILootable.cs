using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Loot
{
    public interface ILootable<TKey, TGenerate>
    {
        int Rarity { get; }
        TKey Key { get; }

        TGenerate Generate();

        /// <summary>returns true if still avaiable after method call</summary>
        bool UpdateAvailability();
    }
}
