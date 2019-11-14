using System.Collections.Generic;

using LootHeresyLib.Algorithms;
using LootHeresyLib.Logger;
using LootHeresyLib.Loot;

namespace LootHeresyLib
{
    public class LootTree<TKey, TGenerate>
    {
        private LootTreeNode<TKey, TGenerate> _root;
        private ILootAlgorithm<TKey, TGenerate> _algo;

        public ILogger Logger { get; set; }
        public LootTreeNode<TKey, TGenerate> Root => _root;

        public LootTree(ILootAlgorithm<TKey, TGenerate> algo, ILogger logger = null)
            => (_root, _algo) = (new LootTreeNode<TKey, TGenerate>(algo, logger), algo);

        public LootTreeNode<TKey, TGenerate> AddPath(ILootable<TKey, TGenerate>[] path)
        => AddPath(_root, path);

        public LootTreeNode<TKey, TGenerate> AddPath(LootTreeNode<TKey, TGenerate> start, params ILootable<TKey, TGenerate>[] path)
        {
            LootTreeNode<TKey, TGenerate> curr = start;
            for (int i = 0; i < path.Length; i++)
            {
                curr = curr.GetOrAddChild(path[i], _algo);
            }
            return curr;
        }

        public LootTreeNode<TKey, TGenerate> GetTreeNodeByPath(IEnumerable<TKey> path)
        {
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
    }
}
