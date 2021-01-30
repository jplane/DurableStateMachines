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
        private readonly IDictionary<string, object> _internalData;
        private readonly object _data;

        public DynamicDictionary(IDictionary<string, object> internalData, object data)
        {
            internalData.CheckArgNull(nameof(internalData));
            data.CheckArgNull(nameof(data));

            _internalData = internalData;
            _data = data;
        }

        private bool TryGetValue(string name, out object result)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(name));

            if (GetObjectValue(name, out result))
            {
                return true;
            }
            else if (GetDictionaryValue(_internalData, name, out result))
            {
                return true;
            }

            result = null;

            return false;
        }

        private bool TrySetValue(string name, object value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(name));

            if (SetObjectValue(name, value))
            {
                return true;
            }
            else if (SetDictionaryValue(_internalData, name, value))
            {
                return true;
            }

            return false;
        }
        private bool GetObjectValue(string name, out object value)
        {
            if (_data is IDictionary<string, object>)
            {
                return GetDictionaryValue((IDictionary<string, object>)_data, name, out value);
            }

            var getter = _data.GetType().GetProperty(name)?.GetGetMethod();
            
            value = getter?.Invoke(_data, null);

            return getter != null;
        }

        private bool SetObjectValue(string name, object value)
        {
            if (_data is IDictionary<string, object>)
            {
                return SetDictionaryValue((IDictionary<string, object>) _data, name, value);
            }

            var setter = _data.GetType().GetProperty(name)?.GetSetMethod();

            setter?.Invoke(_data, new[] { value });

            return setter != null;
        }

        private bool GetDictionaryValue(IDictionary<string, object> dict, string name, out object value)
        {
            return dict.TryGetValue(name, out value);
        }

        private bool SetDictionaryValue(IDictionary<string, object> dict, string name, object value)
        {
            dict[name] = value;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            indexes.CheckArgNull(nameof(indexes));

            if (indexes.Length != 1 || indexes[0] == null || indexes[0].GetType() != typeof(string))
            {
                throw new ExecutionException("Expecting exactly one string-based index for data lookups.");
            }

            var name = (string) indexes[0];

            return TryGetValue(name, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            indexes.CheckArgNull(nameof(indexes));

            if (indexes.Length != 1 || indexes[0] == null || indexes[0].GetType() != typeof(string))
            {
                throw new ExecutionException("Expecting exactly one string-based index for data lookups.");
            }

            var name = (string) indexes[0];

            return TrySetValue(name, value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(binder.Name));

            return TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(binder.Name));

            return TrySetValue(binder.Name, value);
        }

        ICollection<string> IDictionary<string, object>.Keys => _internalData.Keys;

        ICollection<object> IDictionary<string, object>.Values => _internalData.Values;

        int ICollection<KeyValuePair<string, object>>.Count => _internalData.Count;

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => _internalData.IsReadOnly;

        object IDictionary<string, object>.this[string key]
        {
            get => _internalData[key];
            set => _internalData[key] = value;
        }

        void IDictionary<string, object>.Add(string key, object value) => _internalData.Add(key, value);

        bool IDictionary<string, object>.ContainsKey(string key) => _internalData.ContainsKey(key);

        bool IDictionary<string, object>.Remove(string key) => _internalData.Remove(key);

        bool IDictionary<string, object>.TryGetValue(string key, out object value) => _internalData.TryGetValue(key, out value);

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => _internalData.Add(item);

        void ICollection<KeyValuePair<string, object>>.Clear() => _internalData.Clear();

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item) => _internalData.Contains(item);

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => _internalData.CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item) => _internalData.Remove(item);

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator() => _internalData.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _internalData.GetEnumerator();
    }
}
