﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using StateChartsDotNet.Common;

namespace StateChartsDotNet
{
    public class DynamicDictionary : DynamicObject
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

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.Type.IsAssignableFrom(_data.GetType()))
            {
                result = _data;
                return true;
            }

            return base.TryConvert(binder, out result);
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            indexes.CheckArgNull(nameof(indexes));

            if (indexes.Length == 1 && indexes[0] != null && indexes[0].GetType() == typeof(string))
            {
                var name = (string) indexes[0];

                var prop = _data.GetType().GetProperty(name);

                if (prop != null)
                {
                    result = prop.GetValue(_data, null);
                    return true;
                }
                else if (_data is IDictionary<string, object> dict && dict.TryGetValue(name, out result))
                {
                    return true;
                }
                else if (_internalData.TryGetValue(name, out result))
                {
                    return true;
                }
            }

            return base.TryGetIndex(binder, indexes, out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            indexes.CheckArgNull(nameof(indexes));

            if (indexes.Length == 1 &&
                indexes[0] != null &&
                indexes[0].GetType() == typeof(string))
            {
                var name = (string) indexes[0];

                var prop = _data.GetType().GetProperty(name);

                if (prop != null)
                {
                    prop.SetValue(_data, value);
                }
                else if (_data is IDictionary<string, object> dict)
                {
                    dict[name] = value;
                }
                else
                {
                    _internalData[name] = value;
                }

                return true;
            }

            return base.TrySetIndex(binder, indexes, value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(binder.Name));

            var prop = _data.GetType().GetProperty(binder.Name);

            if (prop != null)
            {
                result = prop.GetValue(_data, null);
                return true;
            }
            else if (_data is IDictionary<string, object> dict && dict.TryGetValue(binder.Name, out result))
            {
                return true;
            }
            else if (_internalData.TryGetValue(binder.Name, out result))
            {
                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(binder.Name));

            var prop = _data.GetType().GetProperty(binder.Name);

            if (prop != null)
            {
                prop.SetValue(_data, value);
            }
            else if (_data is IDictionary<string, object> dict)
            {
                dict[binder.Name] = value;
                return true;
            }
            else
            {
                _internalData[binder.Name] = value;
                return true;
            }

            return base.TrySetMember(binder, value);
        }
    }
}
