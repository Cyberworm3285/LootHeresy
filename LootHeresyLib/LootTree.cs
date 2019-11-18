using System;
using System.Collections.Generic;
using System.Collections;

using LootHeresyLib.Algorithms;
using LootHeresyLib.Extensions.Specific;
using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;

namespace LootHeresyLib
{
    public class LootTree<TKey, TGenerate> : IEnumerable<LootTreeNode<TKey, TGenerate>>
    {
        private LootTreeNode<TKey, TGenerate> _root;
        private ILootAlgorithm<TKey, TGenerate> _algo;
        private int _idCounter;

        public ILogger Logger { get; set; }
        public LootTreeNode<TKey, TGenerate> Root => _root;

        public LootTree(ILootAlgorithm<TKey, TGenerate> algo, ILogger logger = null)
        {
            if (algo.IsNull())
                logger.LogAndThrow<ArgumentException>(LoggerSeverity.InputValidation, "no algorithm provided");

            _root = new LootTreeNode<TKey, TGenerate>(0, null, default, 0, algo, logger);
            _idCounter = 1;
            _algo = algo;
        }

        public LootTreeNode<TKey, TGenerate> AddPath(ILootable<TKey, TGenerate>[] path)
        => AddPath(_root, path);

        public LootTreeNode<TKey, TGenerate> AddPath(LootTreeNode<TKey, TGenerate> start, params ILootable<TKey, TGenerate>[] path)
        {
            if (path.IsNull())
                Logger.LogAndThrow<ArgumentException>(LoggerSeverity.InputValidation | LoggerSeverity.PathInfo, "provided path is null");

            LootTreeNode<TKey, TGenerate> curr = start;
            for (int i = 0; i < path.Length; i++)
            {
                curr = curr.GetOrAddChild(path[i], _idCounter, _algo);
                if (curr.ID == _idCounter)
                    _idCounter++;
            }
            return curr;
        }

        public LootTreeNode<TKey, TGenerate> GetTreeNodeByPath(IEnumerable<TKey> path)
        {
            if (path.IsNull())
                Logger.LogAndThrow<ArgumentException>(LoggerSeverity.InputValidation | LoggerSeverity.PathInfo, "path for searching is null");

            LootTreeNode<TKey, TGenerate> curr = _root;
            foreach (var p in path)
                curr = curr.GetChild(p);

            return curr;
        }

        public LootTreeNode<TKey, TGenerate> GetTreeNodeByKey(ILootable<TKey, TGenerate> l)
        => GetTreeNodeByKey(l.Key);

        public LootTreeNode<TKey, TGenerate> GetTreeNodeByKey(TKey k)
        => _root.GetTreeNodeByKey(k);

        public TGenerate GetResult()
        => _root.GetResult();

        #region IEnumerable<T>

        public IEnumerator<LootTreeNode<TKey, TGenerate>> GetEnumerator()
        => Root.TraversePreOrder().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        => Root.TraversePreOrder().GetEnumerator();

        #endregion
    }
}
