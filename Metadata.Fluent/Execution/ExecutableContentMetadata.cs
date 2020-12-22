using StateChartsDotNet.Common.Model;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;

namespace StateChartsDotNet.Metadata.Fluent.Execution
{
    public abstract class ExecutableContentMetadata : IExecutableContentMetadata
    {
        internal ExecutableContentMetadata()
        {
        }

        internal string MetadataId { private get; set; }

        string IModelMetadata.MetadataId => this.MetadataId;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
