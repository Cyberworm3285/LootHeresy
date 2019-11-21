using System;
using System.Collections.Generic;
using System.Linq;

using LootHeresyLib.Loot;
using LootHeresyLib.Logger;
using LootHeresyLib.Algorithms;
using LootHeresyLib.Extensions.Specific;
using LootHeresyLib.Extensions.Generic;

namespace LootHeresyLib.Tree.Nodes
{
    public class ChildNode<TKey, TGenerate> : FallBackNode<TKey, TGenerate>
    {
        private HashSet<ParentNode<TKey, TGenerate>> _parents;

        public override int Layer { get; }

        internal ChildNode(int id, ParentNode<TKey, TGenerate> parent, TKey key, int layer, ILootAlgorithm<TKey, TGenerate> algo, ILogger logger, bool ignoreAvailability = false)
            : base (id, key, algo, logger, ignoreAvailability)
        {
            Layer = layer;
            _algorithm = algo;
            _logger = logger;
            IgnoreAvailability = ignoreAvailability;

            _parents = new HashSet<ParentNode<TKey, TGenerate>> { parent };

            _items = new List<ILootable<TKey, TGenerate>>();
        }

        public ChildNode<TKey, TGenerate> AddLeaf(ILootable<TKey, TGenerate> value)
        {
            if (value == null)
            {
                _logger?.Log(
                    "null value detected in leaf-node, leaf is ignored",
                    LoggerSeverity.Error,
                    this
                );
                return this;
            }
            if (_items.Contains(value))
                _logger?.Log(
                    $"key [{value.Key}] is already registered, new rarity [{value.Rarity}] is ignored",
                    LoggerSeverity.Warning
                );

            _logger?.Log(
                $"{value} registered",
                LoggerSeverity.Info
            );
            _items.Add(value);
            _items.Sort(Comparer<ILootable<TKey, TGenerate>>.Create((a, b) => a.Rarity.CompareTo(b.Rarity)));
            return this;
        }

        public ChildNode<TKey, TGenerate> AddRangeAsLeafs(IEnumerable<ILootable<TKey, TGenerate>> values)
        {
            foreach (var x in values)
            {
                _logger?.Log($"forwarding value [{x.Key}] to {nameof(AddLeaf)}", LoggerSeverity.Info);
                AddLeaf(x);
            }
            return this;
        }

        public ILootable<TKey, TGenerate> GetSpecificWithin(Func<ILootable<TKey, TGenerate>, bool> predicate, ILootAlgorithm<TKey, TGenerate> algo)
        {
            return algo.Generate(_items.Where(x => !_children.ContainsKey(x.Key) && predicate(x)).ToArray());
        }

        internal override void DetachChild(TKey key)
        {
            base.DetachChild(key);

            if (this._items.Count != 0)
                return;
            if (this._fallbackNode != null)
                return;

            _logger?.Log
            (
                $"node underflow, detaching node {this} from parents", 
                LoggerSeverity.Info | LoggerSeverity.Warning |LoggerSeverity.Availability
            );
            _parents.ForEach(x => x.DetachChild(this.Key));
            RaiseOnDetach();
            this.IsDetached = true;
        }

        internal override void RemoveFallback(FallBackNode<TKey, TGenerate> node)
        {
            base.RemoveFallback(node);

            if (_items.Count != 0)
                return;
            if (this._fallbackNode != null)
                return;

            _logger?.Log($"no more fallback and items available, detaching node {this} from parents", LoggerSeverity.Warning);
            _parents.ForEach(x => x.DetachChild(this.Key));
            IsDetached = true;
            RaiseOnDetach();
        }

        public bool AddParent(ChildNode<TKey, TGenerate> p)
        {
            if (_parents.IsNull())
                _logger.LogAndThrow<InvalidOperationException>(LoggerSeverity.PathInfo, "cannot add parent to root", p);

            if (p.IsNull())
                _logger.LogAndThrow<ArgumentException>(LoggerSeverity.InputValidation, "provided parent is null", p);

            return _parents.Add(p);
        }

        #region overrides

        public override string ToString()
        => $"[Node id|{ID}|key|{Key}|layer|{Layer}]";

        #endregion
    }
}
