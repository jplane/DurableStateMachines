using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.Common
{
    public static class DictionaryExtensions
    {
        public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> values)
        {
            dictionary.CheckArgNull(nameof(dictionary));
            values.CheckArgNull(nameof(values));

            foreach (var pair in values)
            {
                dictionary[pair.Key] = pair.Value;
            }
        }
    }
}
