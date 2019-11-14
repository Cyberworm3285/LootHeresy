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

        public ILootAlgorithm<TKey, TGenerate> Algorithm { get; set; }

        internal LootTreeNode(ILootAlgorithm<TKey, TGenerate> algo, ILogger logger)
            => (Algorithm, _logger) = (algo, logger);

        internal LootTreeNode<TKey, TGenerate> GetOrAddChild(ILootable<TKey, TGenerate> l, ILootAlgorithm<TKey, TGenerate> algo)
        {
            if (_children.TryGetValue(l.Key, out var c))
            {
                _logger?.Log($"Child node {l} is already registered, rarity {l.Rarity} is ignored by previous value ({_items.First(x => x.Key.Equals(l.Key)).Rarity})", LoggerSeverity.Warning | LoggerSeverity.PathInfo);
                return c;
            }

            LootTreeNode<TKey, TGenerate> res = new LootTreeNode<TKey, TGenerate>(algo, _logger);
            _items.Add(l);
            _children.Add(l.Key, res);

            return res;
        }

        internal LootTreeNode<TKey, TGenerate> GetChild(TKey key)
        => _children[key];

        public TGenerate GetResult()
        {
            ILootable<TKey, TGenerate> res = Algorithm.Generate(_items.ToArray());
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
    }
}
