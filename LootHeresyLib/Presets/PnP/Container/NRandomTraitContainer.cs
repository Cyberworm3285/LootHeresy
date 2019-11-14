using System;
using System.Linq;

namespace LootHeresyLib.Presets.PnP.Container
{
    public class NRandomTraitContainer : ILootTrait
    {
        private readonly ILootTrait[] _traits;
        private readonly int _n,_k;

        public NRandomTraitContainer(int n, int k, params ILootTrait[] traits)
        {
            if (_n > _k)
                throw new ArgumentException("n > k");

            (_traits, _n, _k) = (traits, n, k);
        }

        public NRandomTraitContainer(int n, params ILootTrait[] traits)
            :this(n, n, traits)
        {

        }

        public string Generate()
        => string.Join(
            "|",
            _traits
                .ToList()
                .OrderBy(x => Rand.Next(_traits.Length))
                .Take(Rand.Next(_n, _k) + 1)
                .Select(x => x.Generate()));
    }
}
