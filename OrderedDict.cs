using System.Collections;
using System.Collections.Generic;

namespace AT_Utils
{
    public class OrderedDict<K, V> : IEnumerable<K>
    {
        private readonly Dictionary<K, V> d = new Dictionary<K, V>();
        private readonly List<K> order = new List<K>();
        
        public void Add(K key, V value)
        {
            if(!d.ContainsKey(key))
                order.Add(key);
            d[key] = value;
        }

        public K Next(K current) => order.Next(current);
        public K Prev(K current) => order.Prev(current);
        
        public V this[K key] => d[key];
        public V this[int i] => d[order[i]];

        public IEnumerator<K> GetEnumerator() => order.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
