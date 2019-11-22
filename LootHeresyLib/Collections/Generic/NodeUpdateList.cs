using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

using LootHeresyLib.Extensions.Generic;
using LootHeresyLib.Tree.Nodes;

namespace LootHeresyLib.Collections.Generic
{
    public class NodeUpdateList<Tkey, TGenerate> :IEnumerable<FallBackNode<Tkey, TGenerate>>, IList<FallBackNode<Tkey, TGenerate>>
    {
        public event Action<FallBackNode<Tkey, TGenerate>> OnDetachedNodeRemoved;
        private readonly List<FallBackNode<Tkey, TGenerate>> _innerList;
        private readonly Dictionary<int, int> _occuranceMap;

        public int Count => _innerList.Count;

        public bool IsReadOnly => false;


        public NodeUpdateList()
        {
            _innerList = new List<FallBackNode<Tkey, TGenerate>>();
            _occuranceMap = new Dictionary<int, int>();
        }

        public NodeUpdateList(IEnumerable<FallBackNode<Tkey, TGenerate>> range)
            : this()
        => AddRange(range);

        private void RemoveFromList(FallBackNode<Tkey, TGenerate> node)
        {
            _innerList.RemoveAll(x => x.ID == node.ID);
            OnDetachedNodeRemoved?.Invoke(node);
        }

        private int IncOccurance(int id)
        {
            if (!_occuranceMap.ContainsKey(id))
            {
                _occuranceMap.Add(id, 1);
                return 1;
            }

            _occuranceMap[id]++;
            return _occuranceMap[id];
        }

        public void Add(FallBackNode<Tkey, TGenerate> node)
        {

            _innerList.Add(node);
            if (IncOccurance(node.ID) == 1)
                node.OnDetach += RemoveFromList;
        }

        public void AddRange(IEnumerable<FallBackNode<Tkey, TGenerate>> range)
        => range.ForEach(Add);

        public bool Remove(FallBackNode<Tkey, TGenerate> node)
        {
            if (_innerList.Remove(node))
            {
                if (--_occuranceMap[node.ID] == 0)
                    node.OnDetach -= RemoveFromList;
                return true;
            }

            return false;
        }

        public int RemoveAll(Func<FallBackNode<Tkey, TGenerate>, bool> predicate)
        => _innerList
            .Where(n => predicate(n) && Remove(n))
            .Count();

        public IEnumerator<FallBackNode<Tkey, TGenerate>> GetEnumerator()
        => _innerList.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        => _innerList.GetEnumerator();

        public int IndexOf(FallBackNode<Tkey, TGenerate> item)
        => _innerList.IndexOf(item);

        public void Insert(int index, FallBackNode<Tkey, TGenerate> item)
        {
            _innerList.Insert(index, item);
            if (IncOccurance(item.ID) == 1)
                item.OnDetach += RemoveFromList;
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            if (--_occuranceMap[index] == 0)
                item.OnDetach -= RemoveFromList;

            _innerList.RemoveAt(index);
        }

        public void Clear()
        {
            _occuranceMap.Clear();
            _innerList
                .Distinct()
                .ForEach(n => n.OnDetach -= RemoveFromList);
            _innerList.Clear();
        }

        public bool Contains(FallBackNode<Tkey, TGenerate> item)
        => _innerList.Contains(item);

        public void CopyTo(FallBackNode<Tkey, TGenerate>[] array, int arrayIndex)
        => _innerList.CopyTo(array, arrayIndex);

        public FallBackNode<Tkey, TGenerate> this[int index]
        {
            get => _innerList[index];
            set
            {
                if (--_occuranceMap[_innerList[index].ID] == 0)
                    _innerList[index].OnDetach -= RemoveFromList;

                _innerList[index] = value;
                if (IncOccurance(value.ID) == 1)
                    value.OnDetach += RemoveFromList;
            }
        }
    }
}
