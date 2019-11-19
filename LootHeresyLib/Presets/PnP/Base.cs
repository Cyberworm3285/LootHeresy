using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Loot;
using System.Collections.Generic;
using System.Linq;

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

        protected IEnumerable<T> InterpretStack<T>(Queue<string> queue, Dictionary<string, T> map)
        {
            foreach (var e in queue.Where(x => x != null))
            {
                if (map.TryGetValue(e, out T r))
                    yield return r;
            }
        }

        public abstract string Generate(Queue<string> generationQueue);
    }
}
