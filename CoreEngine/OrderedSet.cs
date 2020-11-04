using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CoreEngine
{
    internal class OrderedSet<T>
    {
        private readonly SCG.List<T> _items;

        public OrderedSet()
        {
            _items = new SCG.List<T>();
        }

        public static OrderedSet<T> Create(T item)
        {
            var set = new OrderedSet<T>();

            set.Add(item);

            return set;
        }

        public static OrderedSet<T> Create(SCG.IEnumerable<T> items)
        {
            var set = new OrderedSet<T>();

            foreach (var item in items)
            {
                set.Add(item);
            }

            return set;
        }

        public static OrderedSet<T> Union(SCG.IEnumerable<SCG.IEnumerable<T>> collections)
        {
            var set = new OrderedSet<T>();

            foreach (var coll in collections)
            {
                foreach (var item in coll)
                {
                    set.Add(item);
                }
            }

            return set;
        }

        public void Add(T item)
        {
            if (! _items.Contains(item))
            {
                _items.Add(item);
            }
        }

        public void Delete(T item)
        {
            _items.Remove(item);
        }

        public void Union(OrderedSet<T> set)
        {
            foreach (var item in set._items)
            {
                Add(item);
            }
        }

        public void Union(List<T> list)
        {
            foreach (var item in list)
            {
                Add(item);
            }
        }

        public bool IsMember(T item)
        {
            return _items.Contains(item);
        }

        public bool Some(Func<T, bool> predicate)
        {
            if (_items.Count == 0)
            {
                return false;
            }

            return _items.Any(predicate);
        }

        public bool Every(Func<T, bool> predicate)
        {
            if (_items.Count == 0)
            {
                return true;
            }

            return _items.All(predicate);
        }

        public bool HasIntersection(OrderedSet<T> set)
        {
            foreach (var item in set._items)
            {
                if (IsMember(item))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsEmpty()
        {
            return _items.Count == 0;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public List<T> ToList()
        {
            return new List<T>(_items);
        }
    }
}
