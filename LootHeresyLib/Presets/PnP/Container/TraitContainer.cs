using System.Linq;

namespace LootHeresyLib.Presets.PnP.Container
{
    public class TraitContainer : ILootTrait
    {
        private readonly ILootTrait[] _traits;

        public TraitContainer(params ILootTrait[] traits)
        => _traits = traits;

        public string Generate()
        => string.Join("|", _traits.Select(x => x.Generate()));
    }
}
