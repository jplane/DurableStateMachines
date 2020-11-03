using System;
using SCG=System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace CoreEngine
{
    internal class StateChart
    {
        private readonly XDocument _xml;
        private readonly Lazy<XElement[]> _stateElements;
        private readonly Dictionary<string, State> _states;

        public StateChart(XDocument xml)
        {
            _xml = xml;

            _stateElements = new Lazy<XElement[]>(() =>
            {
                SCG.IEnumerable<XElement> GetStates(XElement element)
                {
                    var elements = new SCG.List<XElement>();

                    foreach (var child in element.Descendants())
                    {
                        if (IsStateElement(child))
                        {
                            elements.Add(child);
                            elements.AddRange(GetStates(child));
                        }
                    }

                    return elements;
                }

                return GetStates(_xml.Root).ToArray();
            });

            _states = new Dictionary<string, State>();

            _states.Add("scxml_root", new State(_xml.Root));
        }

        public int CompareDocumentOrder(State state1, State state2)
        {
            return Compare(state1.Id, state2.Id, _stateElements.Value.Select(s => s.Attribute("id").Value));
        }

        public int CompareReverseDocumentOrder(State state1, State state2)
        {
            return Compare(state1.Id, state2.Id, _stateElements.Value.Reverse().Select(s => s.Attribute("id").Value));
        }

        private int Compare(string stateId1, string stateId2, SCG.IEnumerable<string> ids)
        {
            if (string.IsNullOrWhiteSpace(stateId1) && string.IsNullOrWhiteSpace(stateId2))
            {
                return 0;
            }
            else if (string.IsNullOrWhiteSpace(stateId1))
            {
                return -1;
            }
            else if (string.IsNullOrWhiteSpace(stateId2))
            {
                return 1;
            }
            else
            {
                bool AreEqual(string x, string y)
                {
                    return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase) == 0;
                }

                foreach (var id in ids)
                {
                    if (AreEqual(id, stateId1))
                    {
                        return 1;
                    }
                    else if (AreEqual(id, stateId2))
                    {
                        return -1;
                    }
                }

                return 0;
            }
        }

        public static bool IsStateElement(XElement element)
        {
            return element.Name == "state" || element.Name == "parallel" || element.Name == "final";
        }

        public bool IsEarlyBinding
        {
            get => true;
        }

        public SCG.IEnumerable<StateChartData> RootData
        {
            get => GetDataModelForElement(_xml.Root);
        }

        private static XElement GetStateNodeById(XElement parent, string id)
        {
            return parent.Descendants().FirstOrDefault(d => IsStateElement(d) && d.Attribute("id").Value == id);
        }

        public SCG.IEnumerable<(string, SCG.IEnumerable<StateChartData>)> StateData
        {
            get => _stateElements.Value.Select(state =>
            {
                var id = state.Attribute("id").Value;
                return (id, GetDataForState(id));
            });
        }

        public State GetState(string id)
        {
            if (! _states.TryGetValue(id, out State state))
            {
                var element = _stateElements.Value.Single(s => s.Attribute("id").Value == id);

                state = GetState(element);
            }

            return state;
        }

        public State GetState(XElement element)
        {
            var id = element.Attribute("id").Value;

            if (! _states.TryGetValue(id, out State state))
            {
                state = new State(element);

                _states.Add(id, state);
            }

            return state;
        }

        public SCG.IEnumerable<StateChartData> GetDataForState(string id)
        {
            var element = _xml.Element(id);

            if (element == null)
            {
                throw new InvalidOperationException("Unable to resolve element: " + id);
            }

            return GetDataModelForElement(element);
        }

        private SCG.IEnumerable<StateChartData> GetDataModelForElement(XElement element)
        {
            var datamodel = element.Element("datamodel");

            if (datamodel == null)
            {
                return Enumerable.Empty<StateChartData>();
            }
            else
            {
                return datamodel.Elements("data").Select(StateChartData.FromXml);
            }
        }
    }

    internal class StateChartData
    {
        public static StateChartData FromXml(XElement el)
        {
            return new StateChartData
            {
                Id = el.Attribute("id").Value,
                Source = el.Attribute("src").Value,
                Expression = el.Attribute("expr").Value
            };
        }

        public string Id { get; set; }
        public string Source { get; set; }
        public string Expression { get; set; }
    }
}
