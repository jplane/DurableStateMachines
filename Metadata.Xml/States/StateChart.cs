using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Xml.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;


namespace StateChartsDotNet.Metadata.Xml.States
{
    public class StateChart : StateMetadata, IStateChartMetadata
    {
        private readonly string _name;

        public StateChart(XDocument document)
            : base(document.Root)
        {
            _name = _element.Attribute("name").Value;
        }

        public Task<(string, string)> ToStringAsync(CancellationToken cancelToken = default)
        {
            return Task.FromResult(("xml", _element.ToString()));
        }

        public static Task<IStateChartMetadata> FromStringAsync(string content,
                                                                CancellationToken cancelToken = default)
        {
            content.CheckArgNull(nameof(content));

            var xml = XDocument.Parse(content);

            var statechart = new StateChart(xml);

            return Task.FromResult((IStateChartMetadata) statechart);
        }

        public async Task<string> SerializeAsync(Stream stream, CancellationToken cancelToken = default)
        {
            stream.CheckArgNull(nameof(stream));

            using var writer = new StreamWriter(stream, Encoding.UTF8, -1, true);

            await writer.WriteAsync(_element.ToString());

            return "xml";
        }

        public static async Task<IStateChartMetadata> DeserializeAsync(Stream stream,
                                                                       CancellationToken cancelToken = default)
        {
            stream.CheckArgNull(nameof(stream));

            using var reader = new StreamReader(stream, Encoding.UTF8, true, -1, true);

            var doc = XDocument.Parse(await reader.ReadToEndAsync());

            var statechart = new StateChart(doc);

            return statechart;
        }

        public override StateType Type => StateType.Root;

        public override string Id => _name;

        public override string MetadataId => _name;

        public bool FailFast
        {
            get => bool.Parse(_element.Attribute("failfast")?.Value ?? "false");
        }

        public Databinding Databinding
        {
            get => (Databinding) Enum.Parse(typeof(Databinding),
                                            _element.Attribute("binding")?.Value ?? "early",
                                            true);
        }

        public IScriptMetadata GetScript()
        {
            var node = _element.ScxmlElement("script");

            return node == null ? null : (IScriptMetadata)new ScriptMetadata(node);
        }

        public override ITransitionMetadata GetInitialTransition()
        {
            var attr = _element.Attribute("initial");

            if (attr != null)
            {
                return new TransitionMetadata(attr, this.MetadataId);
            }
            else
            {
                var firstChild = this.GetStates().FirstOrDefault();

                return firstChild == null ? null : new TransitionMetadata(firstChild.Id, this.MetadataId);
            }
        }

        public override IEnumerable<IStateMetadata> GetStates()
        {
            var states = new List<IStateMetadata>();

            foreach (var el in _element.Elements())
            {
                if (el.ScxmlNameEquals("parallel"))
                {
                    states.Add(new ParallelStateMetadata(el));
                }
                else if (el.ScxmlNameEquals("final"))
                {
                    states.Add(new FinalStateMetadata(el));
                }
                else if (el.ScxmlNameEquals("state"))
                {
                    states.Add(new StateMetadata(el));
                }
            }

            return states.AsEnumerable();
        }
    }
}
