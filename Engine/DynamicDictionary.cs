using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Exceptions;

namespace StateChartsDotNet
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

            if (indexes.Length != 1 || indexes[0] == null || indexes[0].GetType() != typeof(string))
            {
                throw new ExecutionException("Expecting exactly one string-based index for data lookups.");
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
            Debug.Assert(!string.IsNullOrWhiteSpace(binder.Name));

            if (_data.TryGetValue(binder.Name, out result))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
