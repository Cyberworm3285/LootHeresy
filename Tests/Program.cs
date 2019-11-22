using System;
using System.Linq;
using System.Text;

using LootHeresyLib.Extensions.Specific;
using LootHeresyLib.Algorithms;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;
using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Collections.Generic;

using LootHeresyLib.Presets;
using LootHeresyLib.Tree;

namespace Tests
{
     class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            var random = Rand.GetRandom();
            var logger = new ConsoleLogger
            {
                Take = LoggerSeverity.Error
            };

            var randomLoot = new RandomLoot<string, string>(random) { Logger = logger };
            var partition = new PartitionLoot<string, string>(random) { Logger = logger };
            var fpl = new FirstPickLoot<string, string>();


            LootTree<string, string> tree = new LootTree<string, string>(partition, logger);

            ILootable<string, string>[] convertPath(params string[] s)
            => s.Select(x =>
            {
                if (!x.Contains(':'))
                    return new DefaultLoot<string, string>(50, x, x);

                var parts = x.Split(':');
                return new DefaultLoot<string, string>(int.Parse(parts[1]), parts[0], parts[0]);
            })
            .ToArray();


            var bonusPath           = convertPath("Bonus:5");
            var defaultPath         = convertPath("Default:95");
            var genPath             = convertPath("Default", "Generation");
            var miscPath            = convertPath("Default", "Generation", "Useful", "Misc:70");
            var weaponPath          = convertPath("Default", "Generation", "Useful", "Gear:25"   , "Weapon:35");
            var legendaryWeaponPath = convertPath("Default", "Generation", "Useful", "Gear"      , "LegendaryWeapon:5");
            var armorPath           = convertPath("Default", "Generation", "Useful", "Gear"      , "Armor:60");
            var crapPath            = convertPath("Default", "Generation", "Crap");
            var legacyPath          = convertPath("LegacyGeneration:10");

            var prov = new LootProvider();

            var miscNode = tree.AddPath(miscPath).AddRangeAsLeafs(prov.Misc);
            var crapNode = tree.AddPath(crapPath).AddLeaf(prov.Crap);
            var defaultNode = tree.AddPath(defaultPath);

            var legacyHalfRoot = tree.AddHalfRoot("Legacy");
            var legacyNode = tree
                .AddPath(legacyHalfRoot, legacyPath)
                .AddRangeAsLeafs(prov.Armor)
                .AddRangeAsLeafs(prov.Weapons)
                .AddLeaf(prov.LegendaryWeapon);

            var genNode     = tree.AddPath(genPath);
            var bonusNode   = tree.AddPath(bonusPath);
            var armorNode   = tree.AddPath(armorPath).AddRangeAsLeafs(prov.Armor);
            var weaponNode  = tree.AddPath(weaponPath).AddRangeAsLeafs(prov.Weapons);
            var legyNode    = tree.AddPath(legendaryWeaponPath).AddLeaf(prov.LegendaryWeapon);

            NodeUpdateList<string, string> nodes = tree.AutoList;
            nodes.OnDetachedNodeRemoved += n => Console.WriteLine($"registered node {n} being removed");

            armorNode.SetFallback(weaponNode);
            weaponNode.SetFallback(legyNode);
            nodes.First(x => x.Key == "Gear").SetFallback(legacyHalfRoot);
            tree.Connect(bonusNode, (genNode, 1));
            legacyHalfRoot.AddChildNodes((crapNode, 90));

            legacyNode.IgnoreAvailability = true;

            legacyHalfRoot.Algorithm = fpl;
            bonusNode.Algorithm = fpl;
            defaultNode.Algorithm = fpl;
            crapNode.Algorithm = fpl;

            nodes.ForEach(Console.WriteLine);

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(tree.GetResult());
            }
            nodes.ForEach(Console.WriteLine);
            Console.ReadKey();
        }
    }

    class LootProvider
    {
        public ILootable<string, string> Crap => new Crap(new PartitionLoot<string, string>(Rand.GetRandom()));

        public ILootable<string, string>[] Misc => new ILootable<string, string>[]
        {
            new DynamicLoot("Bullets", (q,t) => $"{Rand.Next(3,7)} Bullets", 0, 3, 25),
            new DynamicLoot("LasAmmo", (q,t) => $"{Rand.Next(2,6)} LasAmmo", 0, 3, 20),
            new DynamicLoot("LesserEuro", (q,t) => $"{Rand.Next(3,10)}€"),
            new DynamicLoot("MajorEuro", (q,t) => $"{Rand.Next(8, 15)}€", 1, 3, 20),
        };

        public ILootable<string, string>[] Armor => new ILootable<string, string>[]
        {
            new DynamicLoot("Light Armor", (q,t) => $"{q.Surround("<{0}> ")}Light Armor {Rand.Next(3,6)}def { t.Surround("[{0}]")}",0,3,60){ Traits = TraitProvider.GenericArmor },
            new DynamicLoot("Medium Armor", (q,t) => $"{q.Surround("<{0}> ")}Medium Armor {Rand.Next(4,8)}def { t.Surround("[{0}]")}",0,2,45){ Traits = TraitProvider.GenericArmor },
            new DynamicLoot("Light Armor", (q,t) => $"{q.Surround("<{0}> ")}Heavy Armor {Rand.Next(7,10)}def { t.Surround("[{0}]")}",0,2,20){ Traits = TraitProvider.GenericArmor },
        };

        public ILootable<string, string>[] Weapons => new ILootable<string, string>[]
        {
            new DynamicLoot("Pistol", (q,t) => $"{q.Surround("<{0}> ")}Pistol 1D8+{Rand.Next(3,5)} Impact dmg {t.Surround("[{0}]")}",0,3,50){ Traits = TraitProvider.GenericRanged },
            new DynamicLoot("LasGun", (q,t) => $"{q.Surround("<{0}> ")}LasGun 6 Energy dmg {t.Surround("[{0}]")}",0,2,40){ Traits = TraitProvider.GenericRanged },
            new DynamicLoot("Bolter", (q,t) => $"{q.Surround("<{0}> ")}Bolter 2D8+{Rand.Next(5,10)} Explosion dmg {t.Surround("[{0}]")}",0,1,10){ Traits = TraitProvider.GenericRanged },
        };

        public ILootable<string, string> LegendaryWeapon
        => new DynamicLoot("LegendarySword", (q,t) => $"{q.Surround("<{0}> ")}Void's Edge 10 NULL dmg {t.Surround("[{0}]")}", 0,1,2) { Traits = TraitProvider.GenericWeaponModsLegendary };
    }
}
