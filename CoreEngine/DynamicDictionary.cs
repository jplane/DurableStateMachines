using System.Collections.Generic;
using System.Dynamic;

namespace CoreEngine
{
    public class DynamicDictionary : DynamicObject
    {
        private readonly Dictionary<string, object> _inner;

        public DynamicDictionary(Dictionary<string, object> inner)
        {
            _inner = inner;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _inner.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _inner[binder.Name] = value;

            return true;
        }
    }
}
