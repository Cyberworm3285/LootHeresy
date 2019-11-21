using System.Linq;
using System.Collections.Generic;
using System;

using LootHeresyLib.Loot;
using LootHeresyLib.Algorithms;
using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Extensions.Specific;

using LootHeresyLib.Presets.PnP.Container;
using LootHeresyLib.Presets.PnP;

namespace Tests
{
    class Crap : DynamicLoot
    {
        public Crap(ILootAlgorithm<string, string> algo)
            : base("Scrap", (q,t) => $"{t.Surround("<{0}>")}{algo.Generate(_craps).Generate(null)}", 2, 8, 25)
        { }

        private static ILootable<string, string>[] Convert(params string[] s)
        => s.Select(x =>
        {
            if (!x.Contains(':'))
                return new DefaultLoot<string, string>(50, x, x);
            string[] parts = x.Split(':');
            return new DefaultLoot<string, string>(int.Parse(parts[1]), parts[0], parts[0]);
        }).ToArray();

        private static ILootable<string, string>[] _craps = Convert(
            "Spoon", "Spork","Dirty Sponge", "Empty Bottle 'o Distilled Bleach", "Zumpel:5",
            "Screws", "Nuts", "Spare Parts"
        );

        public override bool UpdateAvailability() => true;
    }

    static class TraitProvider
    {
        public static readonly ILootTrait GenericWeaponModsLegendary = new TraitContainer(
            new TraitContainer(
                new RandomPlainTrait("Holy", "Unholy", "Xeno", "Earthly"),
                new RandomPlainTrait("Energy", "Sharp", "Blunt"),
                new RandomPlainTrait("Famous", "Feared", "Cursed", "Mythic", "Unknown"),
                new NRandomTraitContainer(
                    1,
                    2,
                    new RandomPlainTrait("Stolen", "Lost In History", "Guided By Fate"),
                    new RandomPlainTrait("Big", "Small", "Ethereal"),
                    new RandomPlainTrait("Loud", "Silent", "Brutal")
                )
            )
        );

        public static readonly ILootTrait GearBonusTrait = new RandomPlainTrait(
                "Indestructable", "Invisible", "Absolute Zero", "Solid <Insert Expensive Material>", "Mass Singularity"
            );

        public static readonly ILootTrait GenericArmor = new RandomPlainTrait(
                null, "Camo", "Battle Scared", "Black", "Disguised"
            );

        public static readonly ILootTrait GenericRanged = new RandomPlainTrait(
                null, "Silenced", "Reliable", "Unreliable", "Precise", "Clunky", "Extended Mag"
            );

        public static readonly Dictionary<string, ILootTrait> DefaultTraitMap = new Dictionary<string, ILootTrait>
        {
            { "Bonus", GearBonusTrait }
        };
    }

    class DynamicLoot : Base
    {
        private Func<string, string, string> _gen;
        private string _name;
        private Dictionary<string, ILootTrait> _map;

        public override string Key => _name;

        public DynamicLoot(string name, Func<string, string, string> gen,int minA = 1, int maxA = 1, int r = 50, Dictionary<string, ILootTrait> traitMap = null)
            : base(minA, maxA, r)
        {
            _gen = gen;
            _name = name;
            _map = traitMap ?? TraitProvider.DefaultTraitMap;
        }

        public override string Generate(Queue<string> generationQueue)
        => _gen($"{string.Join(", ", InterpretQueue(generationQueue, _map).Select(x => x.Generate()))}", Traits.IsNull() ? "" : Traits.Generate());
    } 
}
