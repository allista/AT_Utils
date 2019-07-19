using System;
using System.Collections.Generic;
using System.Linq;

namespace AT_Utils
{
    public static class CollectionsExtensions
    {
        public static TSource SelectMax<TSource>(
            this IEnumerable<TSource> s,
            Func<TSource, float> metric
        )
        {
            float max_v = float.NegativeInfinity;
            TSource max_e = default(TSource);
            foreach(TSource e in s)
            {
                float m = metric(e);
                if(m > max_v)
                {
                    max_v = m;
                    max_e = e;
                }
            }
            return max_e;
        }

        public static void ForEach<TSource>(this IEnumerable<TSource> E, Action<TSource> action)
        {
            if(E != null && action != null)
            {
                var en = E.GetEnumerator();
                while(en.MoveNext())
                    action(en.Current);
            }
        }

        public static void ForEach<TSource>(this IList<TSource> a, Action<TSource> action)
        {
            if(a != null && action != null)
            {
                for(int i = 0, len = a.Count; i < len; i++)
                    action(a[i]);
            }
        }

        public static void ForEach<TSource>(this TSource[] a, Action<TSource> action)
        {
            if(a != null && action != null)
            {
                for(int i = 0, len = a.Length; i < len; i++)
                    action(a[i]);
            }
        }

        public static TSource Pop<TSource>(this LinkedList<TSource> l)
        {
            TSource e = l.Last.Value;
            l.RemoveLast();
            return e;
        }

        public static TSource Min<TSource>(params TSource[] args) where TSource : IComparable
        {
            if(args.Length == 0)
                throw new InvalidOperationException("Min: arguments list should not be empty");
            TSource min = args[0];
            foreach(var arg in args)
            {
                if(min.CompareTo(arg) < 0)
                    min = arg;
            }
            return min;
        }

        public static TSource Max<TSource>(params TSource[] args) where TSource : IComparable
        {
            if(args.Length == 0)
                throw new InvalidOperationException("Max: arguments list should not be empty");
            TSource max = args[0];
            foreach(var arg in args)
            {
                if(max.CompareTo(arg) > 0)
                    max = arg;
            }
            return max;
        }

        public static K Next<K, V>(this SortedList<K, V> list, K key)
        {
            try
            {
                var i = list.IndexOfKey(key);
                var ni = (i + 1) % list.Count;
                return list.Keys[ni];
            }
            catch
            {
                return default(K);
            }
        }

        public static K Prev<K, V>(this SortedList<K, V> list, K key)
        {
            try
            {
                var i = list.IndexOfKey(key);
                var ni = i > 0 ? i - 1 : list.Count - 1;
                return list.Keys[ni];
            }
            catch
            {
                return default(K);
            }
        }

        public static T Next<T>(this IList<T> list, T key)
        {
            try
            {
                var i = list.IndexOf(key);
                var ni = (i + 1) % list.Count;
                return list[ni];
            }
            catch
            {
                return default(T);
            }
        }

        public static T Prev<T>(this IList<T> list, T key)
        {
            try
            {
                var i = list.IndexOf(key);
                var ni = i > 0 ? i - 1 : list.Count - 1;
                return list[ni];
            }
            catch
            {
                return default(T);
            }
        }

        #region Queue extensions
        public static void FillFrom<T>(this Queue<T> q, IEnumerable<T> content)
        {
            q.Clear();
            content.ForEach(q.Enqueue);
        }

        public static bool Remove<T>(this Queue<T> q, T item)
        {
            var count = q.Count;
            var new_content = q.Where(it => !object.Equals(it, item)).ToList();
            q.Clear();
            new_content.ForEach(q.Enqueue);
            return q.Count != count;
        }

        public static bool MoveUp<T>(this Queue<T> q, T up)
        {
            if(object.Equals(up, q.Peek()))
                return false;
            var new_content = q.ToList();
            var upi = new_content.IndexOf(up);
            if(upi < 0)
                return false;
            new_content[upi] = new_content[upi - 1];
            new_content[upi - 1] = up;
            q.Clear();
            new_content.ForEach(q.Enqueue);
            return true;
        }
        #endregion
    }
}
