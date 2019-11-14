using System;
using System.Collections.Generic;
using System.Text;

namespace LootHeresyLib.Presets.PnP.Container
{
    public class RandomPlainTrait : ILootTrait
    {
        private readonly string[] _plainTraits;

        public RandomPlainTrait(params string[] plainTraits)
        => _plainTraits = plainTraits;

        public string Generate()
        => _plainTraits[Rand.Next(_plainTraits.Length)];
    }
}
