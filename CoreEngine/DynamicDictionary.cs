using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;

namespace StateChartsDotNet.CoreEngine
{
    public class DynamicDictionary : DynamicObject
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

            if (indexes.Length != 1 || indexes[0].GetType() != typeof(string))
            {
                throw new InvalidOperationException("Expecting exactly one string-based index for data lookups.");
            }

            if (_data.TryGetValue((string) indexes[0], out result))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_data.TryGetValue(binder.Name, out result))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _data[binder.Name] = value;

            return true;
        }
    }
}
