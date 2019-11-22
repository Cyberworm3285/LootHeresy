using System.Collections.Generic;

using LootHeresyLib.Algorithms;
using LootHeresyLib.Logger;

namespace LootHeresyLib.Tree.Nodes
{
    public class Root<TKey, TGenerate> : ParentNode<TKey, TGenerate>
    {
        private Dictionary<TKey, HalfRoot<TKey, TGenerate>> _halfRoots;
        
        public Root(int id, TKey key, ILootAlgorithm<TKey, TGenerate> algo, ILogger logger, bool ignoreAvailability = false)
            : base(id, key, algo, logger, ignoreAvailability)
        {
            _halfRoots = new Dictionary<TKey, HalfRoot<TKey, TGenerate>>();
        }

        internal bool ContainsHalfRootKey(TKey key)
        => _halfRoots.ContainsKey(key);

        internal bool TryGetHalfRoot(TKey key, out HalfRoot<TKey, TGenerate> n)
        => _halfRoots.TryGetValue(key, out n);

        public bool AddHalfRoot(HalfRoot<TKey, TGenerate> HalfRoot)
        {
            if (_halfRoots.ContainsKey(HalfRoot.Key))
                return false;

            _halfRoots.Add(HalfRoot.Key, HalfRoot);
            return true;
        }

        public bool RemoveHalfRoot(HalfRoot<TKey, TGenerate> HalfRoot)
        => _halfRoots.Remove(HalfRoot.Key);

        public override string ToString()
        => "[Root]";
    }
}
