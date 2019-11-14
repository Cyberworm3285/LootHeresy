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
                        return new DefaultLoot<string, string>(1, x, x);

                    var parts = x.Split(':');
                    return new DefaultLoot<string, string>(int.Parse(parts[1]), parts[0], parts[0]);
                })
                .ToArray();


            var miscPath = convertPath("Useful", "Misc:3");
            var weaponPath = convertPath("Useful", "Gear", "Weapon:2");
            var armorPath = convertPath("Useful", "Gear", "Armor:1");

            var prov = new LootProvider();

            tree.Root.AddLeaf(prov.Crap);
            tree.AddPath(miscPath).AddRangeAsLeafs(prov.Misc);
            tree.AddPath(armorPath).AddRangeAsLeafs(prov.Armor);
            tree.AddPath(weaponPath).AddRangeAsLeafs(prov.Weapons);

            tree.Root.Algorithm = randomLoot;

            tree.ForEach(Console.WriteLine);

            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(tree.GetResult());
            }

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
            new MotherfuckerSwordOfDoom(),
        };
    }
}
