using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using CoreEngine.Model;

namespace CoreEngine
{
    internal class ExecutionState
    {
        private readonly Dictionary<string, object> _data =
            new Dictionary<string, object>();

        public ExecutionState()
        {
        }

        public object this[string key]
        {
            get => _data[key];
            set => _data[key] = value;
        }

        public bool TryGetValue(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }

        public void Init(StateChart statechart)
        {
            foreach(var data in statechart.RootData)
            {
                _data.Add(data.Id, data.Expression);
            }

            foreach (var pair in statechart.StateData)
            {
                foreach (var data in pair.Item2)
                {
                    _data.Add(data.Id, data.Expression);
                }
            }
        }

        public void Init(StateChart statechart, _State state)
        {
            foreach (var pair in statechart.StateData)
            {
                if (pair.Item1 == state.Id)
                {
                    foreach (var data in pair.Item2)
                    {
                        _data.Add(data.Id, data.Expression);
                    }

                    break;
                }
            }

            throw new InvalidOperationException("Unable to resolve state id: " + state.Id);
        }
    }
}
