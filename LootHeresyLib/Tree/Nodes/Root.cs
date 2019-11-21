using System.Collections.Generic;

using LootHeresyLib.Algorithms;
using LootHeresyLib.Logger;

namespace LootHeresyLib.Tree.Nodes
{
    public class Root<TKey, TGenerate> : ParentNode<TKey, TGenerate>
    {
        private Dictionary<TKey, ExoNode<TKey, TGenerate>> _exoNodes;
        
        public Root(int id, TKey key, ILootAlgorithm<TKey, TGenerate> algo, ILogger logger, bool ignoreAvailability = false)
            : base(id, key, algo, logger, ignoreAvailability)
        {
            _exoNodes = new Dictionary<TKey, ExoNode<TKey, TGenerate>>();
        }

        public bool ContainsExoNodeKey(TKey key)
        => _exoNodes.ContainsKey(key);

        public bool TryGetExoNode(TKey key, out ExoNode<TKey, TGenerate> n)
        => _exoNodes.TryGetValue(key, out n);

        public bool AddExoNode(ExoNode<TKey, TGenerate> exoNode)
        {
            if (_exoNodes.ContainsKey(exoNode.Key))
                return false;

            _exoNodes.Add(exoNode.Key, exoNode);
            return true;
        }

        public bool RemoveExoNode(ExoNode<TKey, TGenerate> exoNode)
        => _exoNodes.Remove(exoNode.Key);

        public override string ToString()
        => "[Root]";
    }
}
