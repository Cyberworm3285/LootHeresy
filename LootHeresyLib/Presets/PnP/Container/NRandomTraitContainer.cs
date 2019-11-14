using System.Linq;

namespace LootHeresyLib.Presets.PnP.Container
{
    public class NRandomTraitContainer : ILootTrait
    {
        private readonly ILootTrait[] _traits;
        private readonly int _n;

        public NRandomTraitContainer(int n, params ILootTrait[] traits)
        => (_traits, _n) = (traits, n);

        public string Generate()
        => string.Join(
            "|",
            _traits
                .ToList()
                .OrderBy(x => Rand.Next(_traits.Length))
                .Take(_n)
                .Select(x => x.Generate()));
    }
}
