using System;
using SCG=System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Collections;

namespace CoreEngine
{
    internal class List<T> : SCG.IEnumerable<T>
    {
        private readonly SCG.List<T> _list;

        public List()
        {
            _list = new SCG.List<T>();
        }

        public List(SCG.IEnumerable<T> items)
        {
            _list = new SCG.List<T>(items);
        }

        public static List<T> Create(T item)
        {
            var list = new List<T>();

            list.Append(item);

            return list;
        }

        public void Sort(Comparison<T> func)
        {
            _list.Sort(func);
        }

        public T Head()
        {
            return _list.FirstOrDefault();
        }

        public List<T> Tail()
        {
            return new List<T>(_list.Skip(1));
        }

        public List<T> Append(T item)
        {
            var items = new SCG.List<T>(_list);

            items.Add(item);

            return new List<T>(items);
        }

        public List<T> Filter(Func<T, bool> predicate)
        {
            return new List<T>(_list.Where(predicate));
        }

        public bool Some(Func<T, bool> predicate)
        {
            if (_list.Count == 0)
            {
                return false;
            }

            return _list.Any(predicate);
        }

        public bool Every(Func<T, bool> predicate)
        {
            if (_list.Count == 0)
            {
                return true;
            }

            return _list.All(predicate);
        }

        public SCG.IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
