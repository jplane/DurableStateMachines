using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Common.Model.States;
using StateChartsDotNet.Metadata.Json.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Metadata.Json.States
{
    public class StateChart : StateMetadata, IStateChartMetadata
    {
        private readonly string _name;

        public StateChart(JObject document)
            : base(document)
        {
            _name = _element.Value<string>("name");

            document.InitDocumentPosition();
        }

        public Task<(string, string)> ToStringAsync(CancellationToken cancelToken = default)
        {
            return Task.FromResult(("json", _element.ToString()));
        }

        public static Task<IStateChartMetadata> FromStringAsync(string content,
                                                                CancellationToken cancelToken = default)
        {
            content.CheckArgNull(nameof(content));

            var json = JObject.Parse(content);

            var statechart = new StateChart(json);

            return Task.FromResult((IStateChartMetadata) statechart);
        }

        public async Task<string> SerializeAsync(Stream stream, CancellationToken cancelToken = default)
        {
            stream.CheckArgNull(nameof(stream));

            using var writer = new StreamWriter(stream, Encoding.UTF8, -1, true);

            await writer.WriteAsync(_element.ToString());

            return "json";
        }

        public static async Task<IStateChartMetadata> DeserializeAsync(Stream stream,
                                                                       CancellationToken cancelToken = default)
        {
            stream.CheckArgNull(nameof(stream));

            using var reader = new StreamReader(stream, Encoding.UTF8, true, -1, true);

            var json = JObject.Parse(await reader.ReadToEndAsync());

            var statechart = new StateChart(json);

            return statechart;
        }

        public static Task<IStateChartMetadata> FromJson(JObject json)
        {
            json.CheckArgNull(nameof(json));

            return Task.FromResult((IStateChartMetadata) new StateChart(json));
        }

        public override string Id => _name;

        public override string MetadataId => _name;

        public override StateType Type => StateType.Root;

        public string Debugger => _element.Value<string>("debugger");

        public bool FailFast
        {
            get => bool.Parse(_element.Value<string>("failfast") ?? "false");
        }

        public Databinding Databinding
        {
            get => (Databinding) Enum.Parse(typeof(Databinding),
                                            _element.Value<string>("binding") ?? "early",
                                            true);
        }

        public IScriptMetadata GetScript()
        {
            var node = _element.Property("script")?.Value.Value<JObject>();

            return node == null ? null : (IScriptMetadata)new ScriptMetadata(node);
        }

        public override ITransitionMetadata GetInitialTransition()
        {
            var attr = _element.Value<string>("initial");

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
            var node = _element.Property("states");

            var states = new List<IStateMetadata>();

            foreach (var el in node.Value.Values<JObject>())
            {
                var type = el.Property("type")?.Value.Value<string>();

                if (type == null)
                {
                    states.Add(new StateMetadata(el));
                }
                else if (type == "parallel")
                {
                    states.Add(new ParallelStateMetadata(el));
                }
                else if (type == "final")
                {
                    states.Add(new FinalStateMetadata(el));
                }
            }

            return states.AsEnumerable();
        }
    }
}
