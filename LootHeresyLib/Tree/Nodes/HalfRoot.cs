using LootHeresyLib.Algorithms;
using LootHeresyLib.Logger;

namespace LootHeresyLib.Tree.Nodes
{
    public class HalfRoot<TKey, TGenerate> : FallBackNode<TKey, TGenerate>
    {
        private readonly Root<TKey, TGenerate> _parent;

        public override int Layer => 0;

        internal HalfRoot(int id, TKey key, Root<TKey, TGenerate> parent, ILootAlgorithm<TKey, TGenerate> algo, ILogger logger, bool ignoreAvailability = false)
            : base(id, key, algo, logger, ignoreAvailability)
        {
            _parent = parent;
        }

        internal override void DetachChild(TKey key)
        {
            base.DetachChild(key);

            if (_items.Count != 0 || _fallbackNode != null)
                return;

            _parent.RemoveHalfRoot(this);

            RaiseOnDetach();
            this.IsDetached = true;
        }

        public override string ToString()
        => $"[ExoNode id|{ID}|key|{Key}]";
    }
}
