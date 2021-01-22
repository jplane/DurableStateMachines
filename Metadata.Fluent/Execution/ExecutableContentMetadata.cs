using StateChartsDotNet.Common;
using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public abstract class ExecutableContentMetadata : IExecutableContentMetadata
    {
        internal ExecutableContentMetadata()
        {
        }

        internal string MetadataId { private get; set; }

        string IModelMetadata.MetadataId => this.MetadataId;

        IReadOnlyDictionary<string, object> IModelMetadata.DebuggerInfo => null;

        internal virtual void Serialize(BinaryWriter writer)
        {
            writer.CheckArgNull(nameof(writer));

            writer.WriteNullableString(this.GetType().AssemblyQualifiedName);
            writer.WriteNullableString(this.MetadataId);
        }

        internal static IEnumerable<ExecutableContentMetadata> DeserializeMany(BinaryReader reader, dynamic parent)
        {
            reader.CheckArgNull(nameof(reader));

            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var aqtn = reader.ReadNullableString();

                Debug.Assert(!string.IsNullOrWhiteSpace(aqtn));

                var type = Type.GetType(aqtn);

                Debug.Assert(type != null);

                var deserializeMethod = type.GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Static);

                Debug.Assert(deserializeMethod != null);

                dynamic metadata = (ExecutableContentMetadata)deserializeMethod.Invoke(null, new[] { reader });

                metadata.Parent = parent;

                yield return metadata;
            }
        }
    }
}
