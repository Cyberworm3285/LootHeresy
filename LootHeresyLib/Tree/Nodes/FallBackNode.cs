using System;
using System.Collections.Generic;

using LootHeresyLib.Algorithms;
using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Logger;

namespace LootHeresyLib.Tree.Nodes
{
    public abstract class FallBackNode<TKey, TGenerate> : ParentNode<TKey, TGenerate>
    {
        protected HashSet<ParentNode<TKey, TGenerate>> _linkedByAsFallback;
        public bool IsDetached { get; protected set; }

        public abstract override int Layer { get; }

        public event Action<FallBackNode<TKey, TGenerate>> OnDetach;
        protected void RaiseOnDetach() => OnDetach?.Invoke(this);

        internal FallBackNode(int id, TKey key, ILootAlgorithm<TKey, TGenerate> algo, ILogger logger, bool ignoreAvailability = false)
            : base(id, key, algo, logger, ignoreAvailability)
        {
            IsDetached = false;
            _linkedByAsFallback = new HashSet<ParentNode<TKey, TGenerate>>();
        }

        internal override void DetachChild(TKey key)
        {
            base.DetachChild(key);

            if 
            (
                this._items.Count != 0
                || this._fallbackNode != null
                || this._linkedByAsFallback.Count == 0
            )
                return;

            _logger?.Log($"[{string.Join(",", _linkedByAsFallback)}] are loosing their fallback node", LoggerSeverity.Warning);
            _linkedByAsFallback.ForEach(x => x.RemoveFallback(this));
        }

        internal override void RemoveFallback(FallBackNode<TKey, TGenerate> node)
        {
            base.RemoveFallback(node);
            if (_linkedByAsFallback.Count == 0 || _items.Count != 0)
                return;

            _logger?.Log($"removing node as fallback from [{string.Join(", ", _linkedByAsFallback)}]", LoggerSeverity.Warning);
            _linkedByAsFallback.ForEach(x => x.RemoveFallback(this));
        }

        public void AddAsFallback(ParentNode<TKey, TGenerate> node)
        => _linkedByAsFallback.Add(node);
    }
}
