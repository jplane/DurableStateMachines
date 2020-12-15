using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace StateChartsDotNet
{
    internal class ExternalDictionary : IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _inner;

        public ExternalDictionary(IDictionary<string, object> inner)
        {
            _inner = inner;
        }

        public object this[string key]
        {
            get => key.StartsWith("_") ? throw new KeyNotFoundException() : _inner[key];
            
            set
            {
                if (key.StartsWith("_"))
                {
                    throw new ArgumentException("Keys that begin with underscore are not allowed.");
                }

                _inner[key] = value;
            }
        }

        public ICollection<string> Keys => _inner.Keys.Where(k => !k.StartsWith("_")).ToArray();

        public ICollection<object> Values => _inner.Where(pair => !pair.Key.StartsWith("_"))
                                                   .Select(pair => pair.Value)
                                                   .ToArray();

        public int Count => this.Keys.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            if (key.StartsWith("_"))
            {
                throw new ArgumentException("Keys that begin with underscore are not allowed.");
            }

            _inner.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            if (item.Key.StartsWith("_"))
            {
                throw new ArgumentException("Keys that begin with underscore are not allowed.");
            }

            _inner.Add(item);
        }

        public void Clear()
        {
            _inner.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            if (item.Key.StartsWith("_"))
            {
                return false;
            }

            return _inner.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            if (key.StartsWith("_"))
            {
                return false;
            }

            return _inner.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            var copy = _inner.Where(pair => !pair.Key.StartsWith("_")).ToArray();

            copy.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _inner.Where(pair => !pair.Key.StartsWith("_")).GetEnumerator();
        }

        public bool Remove(string key)
        {
            if (key.StartsWith("_"))
            {
                return false;
            }

            return _inner.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            if (item.Key.StartsWith("_"))
            {
                return false;
            }

            return _inner.Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
        {
            if (key.StartsWith("_"))
            {
                value = null;
                return false;
            }

            return _inner.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
