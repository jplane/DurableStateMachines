using System;
using System.Collections.Generic;
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

        public IReadOnlyDictionary<string, object> ScriptData => _data;

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
}
