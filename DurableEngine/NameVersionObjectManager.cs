using DurableTask.Core;
using System.Collections.Generic;
using System.Diagnostics;

namespace StateChartsDotNet.Durable
{
    internal class NameVersionObjectManager<T> : INameVersionObjectManager<T>
    {
        private readonly IDictionary<string, ObjectCreator<T>> _creators;

        public NameVersionObjectManager()
        {
            _creators = new Dictionary<string, ObjectCreator<T>>();
        }

        public void Clear()
        {
            _creators.Clear();
        }

        public void Add(ObjectCreator<T> creator)
        {
            var key = GetKey(creator.Name, creator.Version);

            lock (_creators)
            {
                Debug.Assert(!_creators.ContainsKey(key));

                _creators.TryAdd(key, creator);
            }
        }

        public T GetObject(string name, string version)
        {
            var key = GetKey(name, version);

            lock (_creators)
            {
                if (_creators.TryGetValue(key, out ObjectCreator<T> creator))
                {
                    return creator.Create();
                }

                return default;
            }
        }

        private string GetKey(string name, string version)
        {
            return $"{name}_{version}";
        }
    }
}
