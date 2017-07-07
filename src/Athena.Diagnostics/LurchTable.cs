using System.Collections.Generic;
using System.Linq;

namespace Athena.Diagnostics
{
    public class LurchTable<TKey, TValue>
    {
        private readonly int _capacity;

        private readonly Dictionary<TKey, LinkedListNode<LurchTableItem<TKey, TValue>>> _cacheMap
            = new Dictionary<TKey, LinkedListNode<LurchTableItem<TKey, TValue>>>();

        private readonly LinkedList<LurchTableItem<TKey, TValue>> _lruList
            = new LinkedList<LurchTableItem<TKey, TValue>>();

        public LurchTable(int capacity)
        {
            _capacity = capacity;
        }

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _cacheMap.ContainsKey(key);
        }

        public TKey[] Keys
        {
            get { return _cacheMap.Select(x => x.Key).ToArray(); }
        }
        
        public TValue Get(TKey key)
        {
            LinkedListNode<LurchTableItem<TKey, TValue>> node;

            if (!_cacheMap.TryGetValue(key, out node))
                return default(TValue);

            var value = node.Value.Value;

            _lruList.Remove(node);
            _lruList.AddLast(node);

            return value;
        }

        public void Add(TKey key, TValue val)
        {
            if (_cacheMap.Count >= _capacity)
                RemoveFirst();

            var cacheItem = new LurchTableItem<TKey, TValue>(key, val);
            var node = new LinkedListNode<LurchTableItem<TKey, TValue>>(cacheItem);

            _lruList.AddLast(node);
            _cacheMap.Add(key, node);
        }

        private void RemoveFirst()
        {
            var node = _lruList.First;
            _lruList.RemoveFirst();

            _cacheMap.Remove(node.Value.Key);
        }
    }
}