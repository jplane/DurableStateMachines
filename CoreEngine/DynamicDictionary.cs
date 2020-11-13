using System.Collections.Generic;
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

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_data.TryGetValue(binder.Name, out result))
            {
                result = Task.FromResult(result);
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
