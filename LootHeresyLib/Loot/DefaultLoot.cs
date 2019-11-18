using System.Collections.Generic;

namespace LootHeresyLib.Loot
{
    public class DefaultLoot<TKey, TGenerate> : ILootable<TKey, TGenerate>
    {
        public int Rarity { get; }
        public TKey Key { get; }
        public TGenerate Item { get; }

        public DefaultLoot(int r, TKey i, TGenerate obj)
        => (Rarity, Key, Item) = (r, i, obj);

        public TGenerate Generate(Stack<TKey> generationStacks)
        => Item;

        public bool UpdateAvailability() => true;

        public override bool Equals(object obj)
        {
            if (obj is DefaultLoot<TKey, TGenerate> dl)
                return Key.Equals(dl.Key);
            return false;
        }

        public override int GetHashCode()
        => Key.GetHashCode();

        public override string ToString()
        => $"[{Key} : {Rarity}]";
    }
}
