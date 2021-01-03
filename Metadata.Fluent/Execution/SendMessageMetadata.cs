using Newtonsoft.Json;
using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Data;
using StateChartsDotNet.Common.Model.Execution;
using StateChartsDotNet.Metadata.Fluent.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class SendMessageMetadata<TParent> : ExecutableContentMetadata, ISendMessageMetadata where TParent : IModelMetadata
    {
        private readonly List<ParamMetadata<SendMessageMetadata<TParent>>> _params;
        private string _id;
        private string _idLocation;
        private TimeSpan _delay;
        private Func<dynamic, TimeSpan> _delayGetter;
        private string _messageName;
        private Func<dynamic, string> _messageNameGetter;
        private string _target;
        private Func<dynamic, string> _targetGetter;
        private string _type;
        private Func<dynamic, string> _typeGetter;
        private object _content;
        private Func<dynamic, object> _contentGetter;

        internal SendMessageMetadata()
        {
            _params = new List<ParamMetadata<SendMessageMetadata<TParent>>>();
            _delay = TimeSpan.Zero;
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteNullableString(_id);
            writer.WriteNullableString(_idLocation);
            writer.WriteNullableString(_delay.ToString());
            writer.Write(_delayGetter);
            writer.WriteNullableString(_messageName);
            writer.Write(_messageNameGetter);
            writer.WriteNullableString(_target);
            writer.Write(_targetGetter);
            writer.WriteNullableString(_type);
            writer.Write(_typeGetter);
            writer.WriteObject(_content);            
            writer.Write(_contentGetter);

            writer.WriteMany(_params, (o, w) => o.Serialize(w));
        }

        internal static SendMessageMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new SendMessageMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();
            
            metadata._id = reader.ReadNullableString();
            metadata._idLocation = reader.ReadNullableString();
            metadata._delay = TimeSpan.Parse(reader.ReadNullableString());
            metadata._delayGetter = reader.Read<Func<dynamic, TimeSpan>>();
            metadata._messageName = reader.ReadNullableString();
            metadata._messageNameGetter = reader.Read<Func<dynamic, string>>();
            metadata._target = reader.ReadNullableString();
            metadata._targetGetter = reader.Read<Func<dynamic, string>>();
            metadata._target = reader.ReadNullableString();
            metadata._typeGetter = reader.Read<Func<dynamic, string>>();
            metadata._content = reader.ReadObject();
            metadata._contentGetter = reader.Read<Func<dynamic, string>>();

            metadata._params.AddRange(reader.ReadMany(ParamMetadata<SendMessageMetadata<TParent>>.Deserialize,
                                                       o => o.Parent = metadata));

            return metadata;
        }

        internal TParent Parent { get; set; }

        public TParent _ => this.Parent;

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
            _delay = timespan;
            _delayGetter = null;
            return this;
        }

        public SendMessageMetadata<TParent> Delay(Func<dynamic, TimeSpan> getter)
        {
            _delayGetter = getter;
            _delay = TimeSpan.Zero;
            return this;
        }

        internal SendMessageMetadata<TParent> MessageName(string messageName)
        {
            _messageName = messageName;
            _messageNameGetter = null;
            return this;
        }

        internal SendMessageMetadata<TParent> MessageName(Func<dynamic, string> getter)
        {
            _messageNameGetter = getter;
            _messageName = null;
            return this;
        }

        internal SendMessageMetadata<TParent> Target(string target)
        {
            _target = target;
            _targetGetter = null;
            return this;
        }

        internal SendMessageMetadata<TParent> Target(Func<dynamic, string> getter)
        {
            _targetGetter = getter;
            _target = null;
            return this;
        }

        internal SendMessageMetadata<TParent> Type(string type)
        {
            _type = type;
            _typeGetter = null;
            return this;
        }

        internal SendMessageMetadata<TParent> Type(Func<dynamic, string> getter)
        {
            _typeGetter = getter;
            _type = null;
            return this;
        }

        internal SendMessageMetadata<TParent> Content(object content)
        {
            _content = content;
            _contentGetter = null;
            return this;
        }

        internal SendMessageMetadata<TParent> Content(Func<dynamic, object> getter)
        {
            _contentGetter = getter;
            _content = null;
            return this;
        }

        internal ParamMetadata<SendMessageMetadata<TParent>> Param(string name)
        {
            var param = new ParamMetadata<SendMessageMetadata<TParent>>(name);

            param.Parent = this;

            _params.Add(param);

            param.MetadataId = $"{((IModelMetadata)this).MetadataId}.Params[{_params.Count}]";

            return param;
        }

        string ISendMessageMetadata.Id => _id;

        string ISendMessageMetadata.IdLocation => _idLocation;

        TimeSpan ISendMessageMetadata.GetDelay(dynamic data) =>
            _delayGetter == null ? _delay : _delayGetter.Invoke(data);

        string ISendMessageMetadata.GetMessageName(dynamic data) =>
            _messageNameGetter == null ? _messageName : _messageNameGetter.Invoke(data);

        object ISendMessageMetadata.GetContent(dynamic data) =>
            _contentGetter == null ? _content : _contentGetter.Invoke(data);

        IReadOnlyDictionary<string, object> ISendMessageMetadata.GetParams(dynamic data) =>
            new ReadOnlyDictionary<string, object>(_params.ToDictionary(p => p.Name, p => p.GetValue(data)));

        string ISendMessageMetadata.GetTarget(dynamic data) =>
            _targetGetter == null ? _target : _targetGetter.Invoke(data);

        string ISendMessageMetadata.GetType(dynamic data) =>
            _typeGetter == null ? _type : _typeGetter.Invoke(data);
    }
}
