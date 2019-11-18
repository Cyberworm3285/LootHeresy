﻿using System;
using System.Collections.Generic;
using System.Linq;

using LootHeresyLib.Loot;
using LootHeresyLib.Logger;
using LootHeresyLib.Algorithms;
using LootHeresyLib.Extensions.Specific;
using LootHeresyLib.Extensions.Generic;

namespace LootHeresyLib
{
    public class LootTreeNode<TKey, TGenerate>
    {
        private Dictionary<TKey, LootTreeNode<TKey, TGenerate>> _children = new Dictionary<TKey, LootTreeNode<TKey, TGenerate>>();
        private List<ILootable<TKey, TGenerate>> _items;
        private LootTreeNode<TKey, TGenerate> _fallbackNode;
        private HashSet<LootTreeNode<TKey, TGenerate>> _linkedByAsFallback;
        private ILogger _logger;
        private LootTreeNode<TKey, TGenerate> _parent;

        private ILootAlgorithm<TKey, TGenerate> _algorithm;

        public readonly int ID;

        public ILootAlgorithm<TKey, TGenerate> Algorithm
        {
            get => _algorithm;
            set => _algorithm = value ?? throw new ArgumentException("algorithm cannot be null");
        }

        public TKey Key { get; }
        public int Layer { get; }

        internal LootTreeNode(int id, LootTreeNode<TKey, TGenerate> parent, TKey key, int layer, ILootAlgorithm<TKey, TGenerate> algo, ILogger logger)
        {
            _parent = parent;
            Key = key;
            Layer = layer;
            _algorithm = algo;
            _logger = logger;
            ID = id;

            _items = new List<ILootable<TKey, TGenerate>>();
            _linkedByAsFallback = new HashSet<LootTreeNode<TKey, TGenerate>>();
        }

        internal LootTreeNode<TKey, TGenerate> GetOrAddChild(ILootable<TKey, TGenerate> l, int nextId, ILootAlgorithm<TKey, TGenerate> algo)
        {
            if (_children.TryGetValue(l.Key, out var c))
            {
                _logger?.Log($"Child node {l} is already registered, rarity {l.Rarity} is ignored by previous value ({_items.First(x => x.Key.Equals(l.Key)).Rarity})", LoggerSeverity.Warning | LoggerSeverity.PathInfo);
                return c;
            }

            LootTreeNode<TKey, TGenerate> res = new LootTreeNode<TKey, TGenerate>(nextId, this, l.Key, Layer + 1, algo, _logger);
            _items.Add(l);
            _children.Add(l.Key, res);

            return res;
        }

        internal LootTreeNode<TKey, TGenerate> GetChild(TKey key)
        => _children[key];

        public TGenerate GetResult()
        => GetResult(new Stack<TKey> { this.Key });

        private TGenerate UseFallback(Stack<TKey> generationStack)
        {
            generationStack.Push(this.Key);
            _logger?.Log($"using fallback to node {_fallbackNode}", LoggerSeverity.Warning);
            return _fallbackNode.GetResult(generationStack);
        }

        private TGenerate GetResult(Stack<TKey> generationStack)
        {
            if (_items.Count == 0)
                return UseFallback(generationStack);

            ILootable<TKey, TGenerate> res = _algorithm.Generate(_items.ToArray());

            if (!res.UpdateAvailability())
                DetachChild(res.Key);

            if (_children.TryGetValue(res.Key, out var t))
            {
                _logger?.Log($"{res.Key} refers to next layer, forwarding..", LoggerSeverity.Info);
                generationStack.Push(this.Key);
                return t.GetResult(generationStack);
            }
            _logger?.Log($"{res} is Leaf, generating output..", LoggerSeverity.Info);
            return res.Generate(generationStack);
        }

        public LootTreeNode<TKey, TGenerate> AddLeaf(ILootable<TKey, TGenerate> value)
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

        public LootTreeNode<TKey, TGenerate> AddRangeAsLeafs(IEnumerable<ILootable<TKey, TGenerate>> values)
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

        internal LootTreeNode<TKey, TGenerate> GetTreeNodeByKey(TKey k)
        {
            if (_children.TryGetValue(k, out var res))
                return res;

            return _children.Values
                .Select(x => x.GetTreeNodeByKey(k))
                .FirstOrDefault(x => x != null);
        }

        internal IEnumerable<LootTreeNode<TKey, TGenerate>> TraversePreOrder()
        {
            yield return this;

            if (this._children.Count == 0)
                yield break;

            foreach (var c in this._children.Values)
                foreach (var r in c.TraversePreOrder())
                    yield return r;
        }

        internal void DetachChild(TKey key)
        {
            this._children.Remove(key);
            this._items.RemoveAll(x => x.Key.Equals(key));
            _logger?.Log
            (
                $"detaching child with key[{key}] in node {this}", 
                LoggerSeverity.Info | LoggerSeverity.Availability
            );

            if (this._items.Count != 0)
                return;
            if (this._fallbackNode != null)
                return;

            if (_parent == null)
            {
                _logger?.Log("root is now empty", LoggerSeverity.Warning | LoggerSeverity.Availability);
                return;
            }

            if (this._linkedByAsFallback.Count != 0)
            {
                _logger?.Log($"[{string.Join(",", _linkedByAsFallback)}] is loosing its fallback node", LoggerSeverity.Warning);
                _linkedByAsFallback.ForEach(x => x.RemoveFallback(this));
            }
            _logger?.Log
            (
                $"node underflow, detaching from next parent {_parent}", 
                LoggerSeverity.Info | LoggerSeverity.Warning |LoggerSeverity.Availability
            );
            _parent.DetachChild(this.Key);
        }

        private bool DetectFallbackLoops(Queue<LootTreeNode<TKey, TGenerate>> traversedNodes)
        {
            if (traversedNodes.Contains(this))
            {
                _logger.LogAndThrow<ArgumentException>(LoggerSeverity.PathInfo, $"cyclic fallback loop detected: {Environment.NewLine}{string.Join(Environment.NewLine, traversedNodes)}{Environment.NewLine}..etc");
                throw new Exception($"this should never be reached, except {nameof(Extensions.Specific.InterfaceExtensions.LogAndThrow)} is broken..");
            }

            traversedNodes.Enqueue(this);
            if (_children.Values.Any(x => x.DetectFallbackLoops(new Queue<LootTreeNode<TKey, TGenerate>>(traversedNodes))))
                return true;

            if (_fallbackNode == null)
                return false;
            return _fallbackNode.DetectFallbackLoops(new Queue<LootTreeNode<TKey, TGenerate>>(traversedNodes));
        }

        public void SetFallback(LootTreeNode<TKey, TGenerate> node)
        {
            var nodeQueue = new Queue<LootTreeNode<TKey, TGenerate>>();
            _fallbackNode = node;

            node.DetectFallbackLoops(nodeQueue);

            node._linkedByAsFallback.Add(this);
        }

        private void RemoveFallback(LootTreeNode<TKey, TGenerate> node)
        {
            if (node != _fallbackNode)
                throw new ArgumentException("node mismatch, this should not be possible");

            _fallbackNode = null;
            if (_items.Count != 0)
                return;

            _logger?.Log($"no more fallback and items available, detaching node {this} from parent", LoggerSeverity.Warning);
            _parent.DetachChild(this.Key);
        }

        #region overrides

        public override string ToString()
        => (_parent == null)
            ? "[Root Layer \"0\"]"
            : $"[Key:\"{Key}\" Layer \"{Layer}\" ID:{ID}]";

        public override bool Equals(object obj)
        {
            if (obj is LootTreeNode<TKey, TGenerate> n)
                return this.ID == n.ID;
            return false;
        }

        public override int GetHashCode()
        => this.ID;

        #endregion
    }
}
