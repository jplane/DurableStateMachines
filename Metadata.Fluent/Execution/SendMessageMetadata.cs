using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.IO;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public sealed class SendMessageMetadata<TParent> : ExecutableContentMetadata, ISendMessageMetadata where TParent : IModelMetadata
    {
        private string _id;
        private string _idLocation;
        private TimeSpan _delay;
        private string _activityType;
        private ISendMessageConfiguration _config;

        internal SendMessageMetadata()
        {
            _delay = TimeSpan.Zero;
        }

        internal override void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            base.Serialize(writer);

            writer.WriteNullableString(_id);
            writer.WriteNullableString(_idLocation);
            writer.WriteNullableString(_delay.ToString());
            writer.WriteNullableString(_activityType);
            writer.WriteObject(_config);
        }

        internal static SendMessageMetadata<TParent> Deserialize(BinaryReader reader)
        {
            reader.CheckArgNull(nameof(reader));

            var metadata = new SendMessageMetadata<TParent>();

            metadata.MetadataId = reader.ReadNullableString();
            
            metadata._id = reader.ReadNullableString();
            metadata._idLocation = reader.ReadNullableString();
            metadata._delay = TimeSpan.Parse(reader.ReadNullableString());
            metadata._activityType = reader.ReadNullableString();
            metadata._config = (ISendMessageConfiguration) reader.ReadObject();

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
            return this;
        }

        public SendMessageMetadata<TParent> ActivityType(string activityType)
        {
            _activityType = activityType;
            return this;
        }

        internal ISendMessageConfiguration Configuration
        {
            get => _config;
            set => _config = value;
        }

        string ISendMessageMetadata.Id => _id;

        string ISendMessageMetadata.IdLocation => _idLocation;

        TimeSpan ISendMessageMetadata.Delay => _delay;

        string ISendMessageMetadata.ActivityType => _activityType;

        ISendMessageConfiguration ISendMessageMetadata.Configuration => _config;
    }
}
