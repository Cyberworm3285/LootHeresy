using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Loot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LootHeresyLib.Presets.PnP
{
    public abstract class Base : ILootable<string, string>
    {
        private int _minAv;

        public ILootTrait Traits { get; set; }
        public virtual int Availability { get; protected set; }
        public virtual int RarityPerItem { get; protected set; }

        public virtual int Rarity => Availability * RarityPerItem;
        public virtual string Key => this.GetType().Name;

        public Base(int minA, int maxA, int rarPerItem)
        {
            _minAv = minA;
            Availability = maxA;
            RarityPerItem = rarPerItem;
        }

        public virtual bool UpdateAvailability()
        {
            Availability = Math.Max(--Availability, _minAv);
            return Availability > 0;
        }

        protected IEnumerable<T> InterpretQueue<T>(Queue<string> queue, Dictionary<string, T> map)
        {
            if (map == null)
                yield break;
            foreach (var e in queue.Where(x => x != null))
            {
                if (map.TryGetValue(e, out T r))
                    yield return r;
            }
        }

        public abstract string Generate(Queue<string> generationQueue);
    }
}
