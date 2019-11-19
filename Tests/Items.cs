using System.Linq;

using LootHeresyLib.Loot;
using LootHeresyLib.Algorithms;

using LootHeresyLib.Presets.PnP.Container;
using LootHeresyLib.Presets.PnP;
using LootHeresyLib.Presets;
using System.Collections.Generic;

namespace Tests
{
    class Crap : BonusBase
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

        public override int Rarity => 1;
        public override string Key => "Crap";

        public override string Generate(Queue<string> generationQueue)
        => base.Generate(generationQueue) + _algo.Generate(_craps).Generate(null);

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
    }

    abstract class BonusBase : Base
    {
        public BonusBase(int availability = -1, int rarPerItem = 50)
            : base(availability, rarPerItem) 
        {

        }

        private static Dictionary<string, ILootTrait> _traitMap = new Dictionary<string, ILootTrait>
        {
            { "Bonus" , TraitProvider.GearBonusTrait }
        };

        public override string Generate(Queue<string> generationQueue)
        {
            var t = InterpretStack<ILootTrait>(generationQueue, _traitMap);
            if (!t.Any())
                return "";
#if DEBUG
            System.Console.BackgroundColor = System.ConsoleColor.Green;
            System.Console.WriteLine("Bonus, wuhu");
            System.Console.ResetColor();
#endif
            return $"[{string.Join(",", t.Select(x => x.Generate()))}]";
        }
    }

    abstract class MiscBase : Base
    {
        public virtual int MinAvailability { get; protected set; }

        public MiscBase(int minAvailability, int maxAvailability, int rarityPeritem)
            : base(maxAvailability, rarityPeritem)
        => MinAvailability = minAvailability;

        public override bool UpdateAvailability()
        {
            if (Avaiability == MinAvailability)
                return true;

            Avaiability--;
            return true;
        }
    }

    class Bullets : MiscBase
    {
        public Bullets() : base(2, 4, 20) { }
        public override string Generate(Queue<string> generationStack) 
            => $"{Rand.Next(2, 8)} Bullets";
    }

    class LasAmmo : MiscBase
    {
        public LasAmmo() : base(1, 2, 15) { }
        public override string Generate(Queue<string> generationStack) 
            => $"{Rand.Next(3, 6)} Las Ammo";
    }

    class Euro : MiscBase
    {
        public Euro() : base(4, 8, 10) { }

        public override string Generate(Queue<string> generationStack) 
            => $"{Rand.Next(4, 10)}€";
    }

    class LightArmor : BonusBase
    {
        public LightArmor() : base(2, 30) { }
        public override string Generate(Queue<string> generationStack) 
            => $"{base.Generate(generationStack)}{Key} +{Rand.Next(2, 5)} Def";
    }

    class MediumArmor : BonusBase
    {
        public MediumArmor() : base(2, 20) { }
        public override string Generate(Queue<string> generationStack) 
            => $"{base.Generate(generationStack)}{Key} +{Rand.Next(3, 6)} Def";
    }

    class HeavyArmor : BonusBase
    {
        public HeavyArmor() : base(1, 15) { }
        public override string Generate(Queue<string> generationStack) 
            => $"{base.Generate(generationStack)}{Key} +{Rand.Next(5, 8)} Def";
    }

    class Pistol : BonusBase
    {
        public Pistol() : base(2, 35) { }
        public override string Generate(Queue<string> generationStack) 
            => $"{base.Generate(generationStack)}{Key} 1D10 + {Rand.Next(2, 5)} bludge dmg";
    }

    class LasGun : BonusBase
    {
        public LasGun() : base(1, 20) { }
        public override string Generate(Queue<string> generationStack) 
            => $"{base.Generate(generationStack)}{Key} 1D10 + 5 energy dmg";
    }

    class MotherfuckerSwordOfDoom : BonusBase
    {
        public MotherfuckerSwordOfDoom()
            : base(1, 1)
        => _traits = TraitProvider.GenericWeaponModsLegendary;

        public override string Generate(Queue<string> generationStack) 
            => $"{base.Generate(generationStack)}{Key} 3D10 + {Rand.Next(7, 10)} dmg [{_traits.Generate()}|Lit af]";
    }

    class Bolter : BonusBase
    {
        public Bolter() : base(1, 5) { }
        public override string Generate(Queue<string> generationStack) 
            => $"{base.Generate(generationStack)}{Key} 2D10 + 7 impact ne explosive";
    }
}
