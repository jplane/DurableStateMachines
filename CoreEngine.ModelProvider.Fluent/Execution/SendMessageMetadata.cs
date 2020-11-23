using StateChartsDotNet.CoreEngine.Abstractions.Model;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;
using StateChartsDotNet.CoreEngine.Abstractions.Model.Execution;
using StateChartsDotNet.CoreEngine.ModelProvider.Fluent.DataManipulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace StateChartsDotNet.CoreEngine.ModelProvider.Fluent.Execution
{
    public sealed class SendMessageMetadata<TParent> : ExecutableContentMetadata, ISendMessageMetadata where TParent : IModelMetadata
    {
        private readonly List<ParamMetadata<SendMessageMetadata<TParent>>> _params;
        private string _id;
        private string _idLocation;
        private ContentMetadata<SendMessageMetadata<TParent>> _content;
        private Func<dynamic, TimeSpan> _delayGetter;
        private Func<dynamic, string> _messageNameGetter;
        private Func<dynamic, string> _targetGetter;
        private Func<dynamic, string> _typeGetter;

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

        public SendMessageMetadata<TParent> Delay(Func<dynamic, TimeSpan> getter)
        {
            _delayGetter = getter;
            return this;
        }

        public SendMessageMetadata<TParent> MessageName(Func<dynamic, string> getter)
        {
            _messageNameGetter = getter;
            return this;
        }

        public SendMessageMetadata<TParent> Target(Func<dynamic, string> getter)
        {
            _targetGetter = getter;
            return this;
        }

        public SendMessageMetadata<TParent> Type(Func<dynamic, string> getter)
        {
            _typeGetter = getter;
            return this;
        }

        public ContentMetadata<SendMessageMetadata<TParent>> Content()
        {
            _content = new ContentMetadata<SendMessageMetadata<TParent>>();

            _content.Parent = this;

            _content.UniqueId = $"{((IModelMetadata)this).UniqueId}.Content";

            return _content;
        }

        public ParamMetadata<SendMessageMetadata<TParent>> Param()
        {
            var param = new ParamMetadata<SendMessageMetadata<TParent>>();

            param.Parent = this;

            _params.Add(param);

            param.UniqueId = $"{((IModelMetadata)this).UniqueId}.Params[{_params.Count}]";

            return param;
        }

        string ISendMessageMetadata.Id => _id;

        string ISendMessageMetadata.IdLocation => _idLocation;

        IContentMetadata ISendMessageMetadata.GetContent() => _content;

        TimeSpan ISendMessageMetadata.GetDelay(dynamic data) => _delayGetter?.Invoke(data);

        string ISendMessageMetadata.GetMessageName(dynamic data) => _messageNameGetter?.Invoke(data);

        IEnumerable<IParamMetadata> ISendMessageMetadata.GetParams() => _params;

        string ISendMessageMetadata.GetTarget(dynamic data) => _targetGetter?.Invoke(data);

        string ISendMessageMetadata.GetType(dynamic data) => _typeGetter?.Invoke(data);
    }
}
