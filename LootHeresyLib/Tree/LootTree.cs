using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using LootHeresyLib.Tree.Nodes;
using LootHeresyLib.Algorithms;
using LootHeresyLib.Extensions.Specific;
using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;
using LootHeresyLib.Collections.Generic;

namespace LootHeresyLib.Tree
{
    public class LootTree<TKey, TGenerate> : IEnumerable<FallBackNode<TKey, TGenerate>>
    {
        private Root<TKey, TGenerate> _root;
        private ILootAlgorithm<TKey, TGenerate> _algo;
        private NodeUpdateList<TKey, TGenerate> _autoList;

        private int _idCounter;

        public ILogger Logger { get; set; }
        public Root<TKey, TGenerate> Root => _root;
        public NodeUpdateList<TKey, TGenerate> AutoList => new NodeUpdateList<TKey, TGenerate>(_autoList);

        public LootTree(ILootAlgorithm<TKey, TGenerate> algo, ILogger logger = null)
        {
            if (algo.IsNull())
                logger.LogAndThrow<ArgumentException>(LoggerSeverity.InputValidation, "no algorithm provided");

            _root = new Root<TKey, TGenerate>(0, default, algo, logger);
            _autoList = new NodeUpdateList<TKey, TGenerate>();

            _idCounter = 1;
            _algo = algo;
        }

        public HalfRoot<TKey, TGenerate> AddHalfRoot(TKey key, ILootAlgorithm<TKey, TGenerate> algo = null, bool ignoreAvailability = false)
        {
            if (Root.TryGetHalfRoot(key, out var n))
                return n;

            var node = new HalfRoot<TKey, TGenerate>
            (
                _idCounter++,
                key,
                Root,
                algo ?? _algo,
                Logger,
                ignoreAvailability
            );
            Root.AddHalfRoot(node);
            _autoList.Add(node);
            return node;
        }

        public ChildNode<TKey, TGenerate> AddPath(ILootable<TKey, TGenerate>[] path)
        => AddPath(_root, path);

        public ChildNode<TKey, TGenerate> AddPath(ParentNode<TKey, TGenerate> start, params ILootable<TKey, TGenerate>[] path)
        {
            if (path.IsNullOrEmpty())
                Logger.LogAndThrow<ArgumentException>(LoggerSeverity.InputValidation | LoggerSeverity.PathInfo, $"provided path is null or empty. use {Root} to access root node");

            ParentNode<TKey, TGenerate> curr = start;
            ChildNode<TKey, TGenerate> res = null;
            for (int i = 0; i < path.Length; i++)
            {
                res = curr.GetOrAddChild(path[i], _idCounter, _algo);
                curr = res; //polymorph madness
                if (curr.ID == _idCounter)
                {

                    _autoList.Add(res);
                    _idCounter++;
                }
            }
            return res;
        }

        public ChildNode<TKey, TGenerate> GetTreeNodeByPath(IEnumerable<TKey> path)
        {
            if (path.IsNull())
                Logger.LogAndThrow<ArgumentException>(LoggerSeverity.InputValidation | LoggerSeverity.PathInfo, "path for searching is null");

            ChildNode<TKey, TGenerate> curr = _root.GetChild(path.First());
            foreach (var p in path.Skip(1))
                curr = curr.GetChild(p);

            return curr;
        }

        public ChildNode<TKey, TGenerate> GetTreeNodeByKey(ILootable<TKey, TGenerate> l)
        => GetTreeNodeByKey(l.Key);

        public ChildNode<TKey, TGenerate> GetTreeNodeByKey(TKey k)
        => _root.GetTreeNodeByKey(k);

        public TGenerate GetResult()
        => _root.GetResult();

        public int Connect(ChildNode<TKey, TGenerate> parent, params (ChildNode<TKey, TGenerate> node, int rarity)[] children)
        {
            children.ForEach(c => c.node.AddParent(parent));
            return parent.AddChildNodes(children);
        }

        public IEnumerable<ChildNode<TKey, TGenerate>> Traverse()
        => Root.TraversePreOrder();

        #region IEnumerable<T>

        public IEnumerator<FallBackNode<TKey, TGenerate>> GetEnumerator()
        => _autoList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        => _autoList.GetEnumerator();

        #endregion
    }
}
