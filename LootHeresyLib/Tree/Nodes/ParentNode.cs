using System;
using System.Collections.Generic;
using System.Linq;

using LootHeresyLib.Algorithms;
using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Extensions.Specific;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;

namespace LootHeresyLib.Tree.Nodes
{
    public abstract class ParentNode<TKey, TGenerate>
    {
        protected Dictionary<TKey, ChildNode<TKey, TGenerate>> _children = new Dictionary<TKey, ChildNode<TKey, TGenerate>>();
        protected FallBackNode<TKey, TGenerate> _fallbackNode;
        protected List<ILootable<TKey, TGenerate>> _items;
        protected ILogger _logger;
        protected ILootAlgorithm<TKey, TGenerate> _algorithm;

        public readonly int ID;

        public bool IgnoreAvailability { get; set; }

        public ILootAlgorithm<TKey, TGenerate> Algorithm
        {
            get => _algorithm;
            set => _algorithm = value ?? throw new ArgumentException("algorithm cannot be null");
        }

        public TKey Key { get; }
        public virtual int Layer => 0;

        internal ParentNode(int id, TKey key, ILootAlgorithm<TKey, TGenerate> algo, ILogger logger, bool ignoreAvailability = false)
        {
            Key = key;
            _algorithm = algo;
            _logger = logger;
            ID = id;
            IgnoreAvailability = ignoreAvailability;

            _items = new List<ILootable<TKey, TGenerate>>();
        }

        internal ChildNode<TKey, TGenerate> GetOrAddChild(ILootable<TKey, TGenerate> l, int nextId, ILootAlgorithm<TKey, TGenerate> algo)
        {
            if (_children.TryGetValue(l.Key, out var c))
            {
                _logger?.Log($"Child node {l} is already registered, rarity {l.Rarity} is ignored by previous value ({_items.First(x => x.Key.Equals(l.Key)).Rarity})", LoggerSeverity.Warning | LoggerSeverity.PathInfo);
                return c;
            }

            ChildNode<TKey, TGenerate> res = new ChildNode<TKey, TGenerate>(nextId, this, l.Key, Layer + 1, algo, _logger);
            _items.Add(l);
            _children.Add(l.Key, res);

            return res;
        }

        internal ChildNode<TKey, TGenerate> GetChild(TKey key)
        => _children[key];

        public TGenerate GetResult()
        => GetResult(new Queue<TKey>());

        private TGenerate UseFallback(Queue<TKey> generationQueue)
        {
            generationQueue.Enqueue(this.Key);
            _logger?.Log($"using fallback to node {_fallbackNode}", LoggerSeverity.Warning);
            return _fallbackNode.GetResult(generationQueue);
        }

        internal TGenerate GetResult(Queue<TKey> generationQueue)
        {
            if (_items.Count == 0)
                if (_fallbackNode.IsNull())
                    _logger.LogAndThrow<Exception>(LoggerSeverity.PathInfo, "no items and no fallback registered, there is nowhere left to go", this);
                else
                    return UseFallback(generationQueue);

            ILootable<TKey, TGenerate> res = _algorithm.Generate(_items.ToArray());

            if (!IgnoreAvailability && !res.UpdateAvailability())
                DetachChild(res.Key);

            if (_children.TryGetValue(res.Key, out var t))
            {
                _logger?.Log($"{res.Key} refers to next layer, forwarding..", LoggerSeverity.Info);
                generationQueue.Enqueue(this.Key);
                return t.GetResult(generationQueue);
            }
            _logger?.Log($"{res} is Leaf, generating output..", LoggerSeverity.Info);
            return res.Generate(generationQueue);
        }

        internal ChildNode<TKey, TGenerate> GetTreeNodeByKey(TKey k)
        {
            if (_children.TryGetValue(k, out var res))
                return res;

            return _children.Values
                .Select(x => x.GetTreeNodeByKey(k))
                .FirstOrDefault(x => x != null);
        }

        internal IEnumerable<ChildNode<TKey, TGenerate>> TraversePreOrder()
        {
            if (this._children.Count == 0)
                yield break;

            foreach (var c in this._children.Values)
                foreach (var r in c.TraversePreOrder())
                    yield return r;
        }

        internal virtual void DetachChild(TKey key)
        {
            this._children.Remove(key);
            this._items.RemoveAll(x => x.Key.Equals(key));
            _logger?.Log
            (
                $"detaching child with key[{key}] in node {this}",
                LoggerSeverity.Info | LoggerSeverity.Availability
            );
        }

        internal virtual void RemoveFallback(FallBackNode<TKey, TGenerate> node)
        {
            if (node != _fallbackNode)
                throw new ArgumentException("node mismatch, this should not be possible");

            _fallbackNode = null;
        }

        protected bool DetectReferenceLoops(Queue<ParentNode<TKey, TGenerate>> traversedNodes)
        {
            if (traversedNodes.Contains(this))
            {
                _logger.LogAndThrow<ArgumentException>(LoggerSeverity.PathInfo, $"cyclic fallback loop detected: {Environment.NewLine}{string.Join(Environment.NewLine, traversedNodes)}{Environment.NewLine}..etc");
                throw new Exception($"this should never be reached, except {nameof(Extensions.Specific.InterfaceExtensions.LogAndThrow)} is broken..");
            }

            traversedNodes.Enqueue(this);
            if (_children.Values.Any(x => x.DetectReferenceLoops(new Queue<ParentNode<TKey, TGenerate>>(traversedNodes))))
                return true;

            if (_fallbackNode == null)
                return false;
            return _fallbackNode.DetectReferenceLoops(new Queue<ParentNode<TKey, TGenerate>>(traversedNodes));
        }

        public void SetFallback(FallBackNode<TKey, TGenerate> node)
        {
            var nodeQueue = new Queue<ParentNode<TKey, TGenerate>>();
            _fallbackNode = node;

            node.DetectReferenceLoops(nodeQueue);

            node.AddAsFallback(this);
        }

        public int AddChildNodes(params (ChildNode<TKey, TGenerate> node, int rarity)[] cn)
        {
            if (cn.IsNullOrEmpty() || cn.Any(x => x.IsNull()))
                _logger.LogAndThrow<ArgumentException>(LoggerSeverity.InputValidation, "provided nodes are null, empty or contain null values", cn);

            int counter = 0;
            foreach (var n in cn)
            {
                if (_children.ContainsKey(n.node.Key))
                    continue;

                DefaultLoot<TKey, TGenerate> lootable = new DefaultLoot<TKey, TGenerate>(n.rarity, n.node.Key, default);
                _items.Add(lootable);
                _children.Add(n.node.Key, n.node);
                counter++;
            }
            DetectReferenceLoops(new Queue<ParentNode<TKey, TGenerate>>());
            return counter;
        }

        #region overrides

        public override string ToString()
        => $"[Parent id|{ID}|layer|{Layer}|key|{Key}]";

        public override bool Equals(object obj)
        {
            if (obj is ChildNode<TKey, TGenerate> n)
                return this.ID == n.ID;
            return false;
        }

        public override int GetHashCode()
        => this.ID;

        #endregion
    }
}
