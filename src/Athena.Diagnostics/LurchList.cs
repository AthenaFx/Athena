using System.Collections;
using System.Collections.Generic;

namespace Athena.Diagnostics
{
    public class LurchList<TValue> : IEnumerable<TValue>
    {
        private readonly int _capacity;

        private readonly LinkedList<TValue> _lruList = new LinkedList<TValue>();

        public LurchList(int capacity)
        {
            _capacity = capacity;
        }
        
        public void AddFirst(TValue val)
        {
            _lruList.AddFirst(val);
            
            if (_lruList.Count >= _capacity)
                _lruList.RemoveLast();
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return _lruList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}