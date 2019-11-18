using LootHeresyLib.Loot;
using System.Collections.Generic;

namespace LootHeresyLib.Presets.PnP
{
    public abstract class Base : ILootable<string, string>
    {
        public virtual int Avaiability { get; protected set; }
        public virtual int RarityPerItem { get; protected set; }
        protected ILootTrait _traits;

        public virtual int Rarity => Avaiability * RarityPerItem;
        public virtual string Key => this.GetType().Name;

        public Base(int availability = -1, int rarPerItem = 50)
        => (Avaiability, RarityPerItem) = (availability, rarPerItem); 

        public virtual bool UpdateAvailability()
        {
            if (Avaiability < 0)
                return true;
            if (Avaiability == 0)
                return false;

            return --Avaiability > 0;
        }

        public abstract string Generate(Stack<string> generationStack);
    }
}
