using System;
using System.Collections.Generic;
using System.Linq;

using LootHeresyLib.Loot;
using LootHeresyLib.Logger;
using LootHeresyLib.Algorithms;

namespace LootHeresyLib
{
    public class LootTreeNode<TKey, TGenerate>
    {
        private Dictionary<TKey, LootTreeNode<TKey, TGenerate>> _children = new Dictionary<TKey, LootTreeNode<TKey, TGenerate>>();
        private List<ILootable<TKey, TGenerate>> _items = new List<ILootable<TKey, TGenerate>>();
        private ILogger _logger;
        private LootTreeNode<TKey, TGenerate> _parent;

        public ILootAlgorithm<TKey, TGenerate> Algorithm { get; set; }
        public TKey Key { get; }
        public int Layer { get; }

        internal LootTreeNode(LootTreeNode<TKey, TGenerate> parent, TKey key, int layer, ILootAlgorithm<TKey, TGenerate> algo, ILogger logger)
            => (_parent, Key, Layer, Algorithm, _logger) = (parent, key, layer, algo, logger);

        internal LootTreeNode<TKey, TGenerate> GetOrAddChild(ILootable<TKey, TGenerate> l, ILootAlgorithm<TKey, TGenerate> algo)
        {
            if (_children.TryGetValue(l.Key, out var c))
            {
                _logger?.Log($"Child node {l} is already registered, rarity {l.Rarity} is ignored by previous value ({_items.First(x => x.Key.Equals(l.Key)).Rarity})", LoggerSeverity.Warning | LoggerSeverity.PathInfo);
                return c;
            }

            LootTreeNode<TKey, TGenerate> res = new LootTreeNode<TKey, TGenerate>(this, l.Key, Layer + 1, algo, _logger);
            _items.Add(l);
            _children.Add(l.Key, res);

            return res;
        }

        internal LootTreeNode<TKey, TGenerate> GetChild(TKey key)
        => _children[key];

        public TGenerate GetResult()
        {
            ILootable<TKey, TGenerate> res = Algorithm.Generate(_items.ToArray());

            if (!res.UpdateAvaiability())
                DetachChild(res.Key);

            if (_children.TryGetValue(res.Key, out var t))
            {
                _logger?.Log($"{res.Key} refers to next layer, forwarding..", LoggerSeverity.Info);
                return t.GetResult();
            }
            _logger?.Log($"{res} is Leaf, generating output..", LoggerSeverity.Info);
            return res.Generate();
        }

        public void AddLeaf(ILootable<TKey, TGenerate> value)
        {
            if (value == null)
            {
                _logger?.Log(
                    "null value detected in leaf-node",
                    LoggerSeverity.Error,
                    this
                );
                return;
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
        }

        public void AddRangeAsLeafs(IEnumerable<ILootable<TKey, TGenerate>> values)
        {
            foreach (var x in values)
            {
                _logger?.Log($"forwarding value [{x.Key}] to {nameof(AddLeaf)}", LoggerSeverity.Info);
                AddLeaf(x);
            }
        }

        public ILootable<TKey, TGenerate> GetSpecificWithin(Func<ILootable<TKey, TGenerate>, bool> predicate, ILootAlgorithm<TKey, TGenerate> algo)
        {
            return algo.Generate(_items.Where(x => !_children.ContainsKey(x.Key) && predicate(x)).ToArray());
        }

        internal LootTreeNode<TKey, TGenerate> GetTreeNodeByKey(TKey o)
        {
            if (_children.TryGetValue(o, out var res))
                return res;

            return _children.Values
                .Select(x => x.GetTreeNodeByKey(o))
                .FirstOrDefault(x => x != null);
        }

        internal IEnumerable<LootTreeNode<TKey, TGenerate>> TraversePreOrderWhere(Predicate<LootTreeNode<TKey, TGenerate>> predicate)
        {
            if (predicate(this))
                yield return this;

            if (this._children.Count == 0)
                yield break;

            foreach (var c in this._children.Values)
                foreach (var r in c.TraversePreOrderWhere(predicate))
                    yield return r;
        }

        internal void DetachChild(TKey key)
        {
            this._children.Remove(key);
            this._items.RemoveAll(x => x.Key.Equals(key));
            _logger?.Log
            (
                $"detaching child with key[{key}] in node [{this.Key}], located in layer {Layer}", 
                LoggerSeverity.Info | LoggerSeverity.Avaiability
            );

            if (this._items.Count != 0)
                return;

            _logger?.Log
            (
                $"node underflow, detaching from next parent \"{_parent.Key}\"", 
                LoggerSeverity.Info | LoggerSeverity.Warning |LoggerSeverity.Avaiability
            );
            _parent.DetachChild(this.Key);
        }
    }
}
