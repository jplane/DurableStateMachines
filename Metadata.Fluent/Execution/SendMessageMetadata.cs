using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Metadata.Fluent.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class SendMessageMetadata<TParent> : ExecutableContentMetadata, ISendMessageMetadata where TParent : IModelMetadata
    {
        private readonly List<ParamMetadata<SendMessageMetadata<TParent>>> _params;
        private string _id;
        private string _idLocation;
        private Func<dynamic, TimeSpan> _delayGetter;
        private Func<dynamic, string> _messageNameGetter;
        private Func<dynamic, string> _targetGetter;
        private Func<dynamic, string> _typeGetter;
        private Func<dynamic, object> _contentGetter;

        internal SendMessageMetadata()
        {
            _params = new List<ParamMetadata<SendMessageMetadata<TParent>>>();
        }

        internal TParent Parent { get; set; }

        public TParent Attach()
        {
            return this.Parent;
        }

        public SendMessageMetadata<TParent> Id(string id)
        {
            _id = id;
            return this;
        }

        public SendMessageMetadata<TParent> IdLocation(string idLocation)
        {
            _idLocation = idLocation;
            return this;
        }

        public SendMessageMetadata<TParent> Delay(TimeSpan timespan)
        {
            _delayGetter = _ => timespan;
            return this;
        }

        public SendMessageMetadata<TParent> Delay(Func<dynamic, TimeSpan> getter)
        {
            _delayGetter = getter;
            return this;
        }

        public SendMessageMetadata<TParent> MessageName(string messageName)
        {
            _messageNameGetter = _ => messageName;
            return this;
        }

        public SendMessageMetadata<TParent> MessageName(Func<dynamic, string> getter)
        {
            _messageNameGetter = getter;
            return this;
        }

        public SendMessageMetadata<TParent> Target(string target)
        {
            _targetGetter = _ => target;
            return this;
        }

        public SendMessageMetadata<TParent> Target(Func<dynamic, string> getter)
        {
            _targetGetter = getter;
            return this;
        }

        public SendMessageMetadata<TParent> Type(string type)
        {
            _typeGetter = _ => type;
            return this;
        }

        public SendMessageMetadata<TParent> Type(Func<dynamic, string> getter)
        {
            _typeGetter = getter;
            return this;
        }

        public SendMessageMetadata<TParent> Content(object content)
        {
            _contentGetter = _ => content;
            return this;
        }

        public SendMessageMetadata<TParent> Content(Func<dynamic, object> getter)
        {
            _contentGetter = getter;
            return this;
        }

        public ParamMetadata<SendMessageMetadata<TParent>> Param(string name)
        {
            var param = new ParamMetadata<SendMessageMetadata<TParent>>(name);

            param.Parent = this;

            _params.Add(param);

            param.UniqueId = $"{((IModelMetadata)this).UniqueId}.Params[{_params.Count}]";

            return param;
        }

        string ISendMessageMetadata.Id => _id;

        string ISendMessageMetadata.IdLocation => _idLocation;

        TimeSpan ISendMessageMetadata.GetDelay(dynamic data) => _delayGetter?.Invoke(data);

        string ISendMessageMetadata.GetMessageName(dynamic data) => _messageNameGetter?.Invoke(data);

        object ISendMessageMetadata.GetContent(dynamic data) => _contentGetter?.Invoke(data);

        IReadOnlyDictionary<string, object> ISendMessageMetadata.GetParams(dynamic data) =>
            new ReadOnlyDictionary<string, object>(_params.ToDictionary(p => p.Name, p => p.GetValue(data)));

        string ISendMessageMetadata.GetTarget(dynamic data) => _targetGetter?.Invoke(data);

        string ISendMessageMetadata.GetType(dynamic data) => _typeGetter?.Invoke(data);
    }
}
