using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StateChartsDotNet.Common;

namespace StateChartsDotNet
{
    internal class Set<T> : IEnumerable<T>
    {
        private readonly List<T> _items = new List<T>();

        public Set()
        {
        }

        public Set(IEnumerable<T> items)
        {
            items.CheckArgNull(nameof(items));

            foreach (var item in items)
            {
                this.Add(item);
            }
        }

        public void Add(T item)
        {
            if (!_items.Contains(item))
            {
                _items.Add(item);
            }
        }

        public void Remove(T item)
        {
            _items.Remove(item);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public void Union(IEnumerable<T> items)
        {
            items.CheckArgNull(nameof(items));

            foreach (var item in items)
            {
                Add(item);
            }
        }

        public bool HasIntersection(IEnumerable<T> items)
        {
            items.CheckArgNull(nameof(items));

            if (IsEmpty())
            {
                return false;
            }

            return items.Any(item => _items.Contains(item));
        }

        public bool IsEmpty()
        {
            return _items.Count == 0;
        }

        public IEnumerable<T> Sort(Comparison<T> comparison)
        {
            comparison.CheckArgNull(nameof(comparison));

            var list = _items.ToArray().ToList();   // make a copy

            list.Sort(comparison);

            return list;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
