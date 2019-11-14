using LootHeresyLib.Loot;

namespace LootHeresyLib.Presets.PnP
{
    public abstract class Base : ILootable<string, string>
    {
        protected virtual int _avaiability { get; set; } = 1;
        protected ILootTrait _traits;

        public virtual int Rarity => 1;
        public virtual string Key => this.GetType().Name;

        public virtual bool UpdateAvaiability()
        {
            if (_avaiability < 0)
                return true;
            if (_avaiability == 0)
                return false;

            return --_avaiability > 0;
        }

        public abstract string Generate();

    }
}
