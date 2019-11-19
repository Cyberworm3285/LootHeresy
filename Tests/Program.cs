using System;
using System.Linq;
using System.Text;

using LootHeresyLib;
using LootHeresyLib.Algorithms;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;
using LootHeresyLib.Extensions.Generic;

using LootHeresyLib.Presets;

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
                Take = LoggerSeverity.Availability | LoggerSeverity.Error | LoggerSeverity.Warning,
            };

            var randomLoot = new RandomLoot<string, string>(random) { Logger = logger };
            var partition = new PartitionLoot<string, string>(random) { Logger = logger };


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

            var prov = new LootProvider();

            tree.AddPath(miscPath).AddRangeAsLeafs(prov.Misc);
            tree.AddPath(crapPath).AddLeaf(prov.Crap);
            tree.AddPath(defaultPath);

            var genNode     = tree.AddPath(genPath);
            var bonusNode   = tree.AddPath(bonusPath);
            var armorNode   = tree.AddPath(armorPath).AddRangeAsLeafs(prov.Armor);
            var weaponNode  = tree.AddPath(weaponPath).AddRangeAsLeafs(prov.Weapons);
            var legyNode    = tree.AddPath(legendaryWeaponPath).AddLeaf(prov.LegendaryWeapon); ;


            armorNode.SetFallback(weaponNode);
            weaponNode.SetFallback(legyNode);
            tree.Connect(bonusNode, (genNode, 1));

            tree
                .Distinct()
                .ForEach(Console.WriteLine);

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(tree.GetResult());
            }
            tree
                .Distinct()
                .ForEach(Console.WriteLine);
            Console.ReadKey();
        }
    }

    class LootProvider
    {
        public ILootable<string, string> Crap => new Crap(new PartitionLoot<string, string>(Rand.GetRandom()));

        public ILootable<string, string>[] Misc => new ILootable<string, string>[]
        {
            new Bullets(),
            new LasAmmo(),
            new Euro(),
        };

        public ILootable<string, string>[] Armor => new ILootable<string, string>[]
        {
            new LightArmor(),
            new MediumArmor(),
            new HeavyArmor(),
        };

        public ILootable<string, string>[] Weapons => new ILootable<string, string>[]
        {
            new Pistol(),
            new LasGun(),
            new Bolter(),
        };

        public ILootable<string, string> LegendaryWeapon => new MotherfuckerSwordOfDoom();
    }
}
