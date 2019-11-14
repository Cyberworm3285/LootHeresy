using LootHeresyLib.Loot;

namespace LootHeresyLib.Presets.PnP
{
    public abstract class Base : ILootable<string, string>
    {
        public virtual int Rarity => 1;
        public virtual string Key => this.GetType().Name;
        public abstract string Generate();

        protected ILootTrait _traits;
    }
}
