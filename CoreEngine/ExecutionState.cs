using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Xml.Linq;
using CoreEngine.Model;
using CoreEngine.Model.States;

namespace CoreEngine
{
    internal class ExecutionState
    {
        private readonly Dictionary<string, object> _data =
            new Dictionary<string, object>();

        public ExecutionState()
        {
        }

        public DynamicDictionary ScriptData => new DynamicDictionary(_data);

        public object this[string key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        public bool TryGetValue(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }
    }

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
