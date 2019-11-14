using System.Linq;

using LootHeresyLib.Loot;
using LootHeresyLib.Algorithms;

using LootHeresyLib.Presets.PnP.Container;
using LootHeresyLib.Presets.PnP;
using LootHeresyLib.Presets;

namespace Tests
{
    class Crap : ILootable<string, string>
    {
        private ILootAlgorithm<string, string> _algo;

        public Crap(ILootAlgorithm<string, string> algo)
        => _algo = algo;

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

        public int Rarity => 1;
        public string Key => "Crap";

        public string Generate()
        => _algo.Generate(_craps).Generate();

        public bool UpdateAvailability() => true;
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
                    new RandomPlainTrait("Stolen", "Lost In History", "Guided By Fate"),
                    new RandomPlainTrait("Big", "Small", "Ethereal"),
                    new RandomPlainTrait("Loud", "Silent", "Brutal")
                )
            )
        );
    }

    class Bullets : Base
    {
        public override string Generate() => $"{Rand.Next(2, 8)} Bullets";
    }

    class LasAmmo : Base
    {
        public override string Generate() => $"{Rand.Next(3, 6)} Las Ammo";
    }

    class Euro : Base
    {
        public override int Rarity => 3;
        public override string Generate() => $"{Rand.Next(4, 10)}€";
    }

    class LightArmor : Base
    {
        public override int Rarity => 4;
        public override string Generate() => $"{Key} +{Rand.Next(2, 5)} Def";
    }

    class MediumArmor : Base
    {
        public override int Rarity => 3;
        public override string Generate() => $"{Key} +{Rand.Next(3, 6)} Def";
    }

    class HeavyArmor : Base
    {
        public override string Generate() => $"{Key} +{Rand.Next(5, 8)} Def";
    }

    class Pistol : Base
    {
        public override int Rarity => 15;
        public override string Generate() => $"{Key} 1D10 + {Rand.Next(2, 5)} bludge dmg";
    }

    class LasGun : Base
    {
        public override int Rarity => 13;
        public override string Generate() => $"{Key} 1D10 + 5 energy dmg";
    }

    class MotherfuckerSwordOfDoom : Base
    {
        public override string Generate() => $"{Key} 3D10 + {Rand.Next(7, 10)} dmg [{_traits.Generate()}|Lit af]";

        public MotherfuckerSwordOfDoom()
        => _traits = TraitProvider.GenericWeaponModsLegendary;
    }
}
