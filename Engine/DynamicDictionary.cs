using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;

namespace StateChartsDotNet
{
    public class DynamicDictionary : DynamicObject, IDictionary<string, object>
    {
        private readonly IDictionary<string, object> _data;

        public DynamicDictionary(IDictionary<string, object> data)
        {
            data.CheckArgNull(nameof(data));

            _data = data;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            indexes.CheckArgNull(nameof(indexes));

            if (indexes.Length != 1 || indexes[0] == null || indexes[0].GetType() != typeof(string))
            {
                throw new ExecutionException("Expecting exactly one string-based index for data lookups.");
            }

            if (! _data.TryGetValue((string) indexes[0], out result))
            {
                result = null;
            }

            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            indexes.CheckArgNull(nameof(indexes));

            if (indexes.Length != 1 || indexes[0] == null || indexes[0].GetType() != typeof(string))
            {
                throw new ExecutionException("Expecting exactly one string-based index for data lookups.");
            }

            _data[(string) indexes[0]] = value;

            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(binder.Name));

            if (! _data.TryGetValue(binder.Name, out result))
            {
                result = null;
            }

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(binder.Name));

            _data[binder.Name] = value;

            return true;
        }

        ICollection<string> IDictionary<string, object>.Keys => _data.Keys;

        ICollection<object> IDictionary<string, object>.Values => _data.Values;

        int ICollection<KeyValuePair<string, object>>.Count => _data.Count;

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => _data.IsReadOnly;

        object IDictionary<string, object>.this[string key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        void IDictionary<string, object>.Add(string key, object value) => _data.Add(key, value);

        bool IDictionary<string, object>.ContainsKey(string key) => _data.ContainsKey(key);

        bool IDictionary<string, object>.Remove(string key) => _data.Remove(key);

        bool IDictionary<string, object>.TryGetValue(string key, out object value) => _data.TryGetValue(key, out value);

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => _data.Add(item);

        void ICollection<KeyValuePair<string, object>>.Clear() => _data.Clear();

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => _data.Contains(item);

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _data.CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) => _data.Remove(item);

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
    }
}
