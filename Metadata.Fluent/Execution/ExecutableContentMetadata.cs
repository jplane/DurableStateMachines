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

        internal string UniqueId { private get; set; }

        string IModelMetadata.UniqueId => this.UniqueId;

        bool IModelMetadata.Validate(Dictionary<IModelMetadata, List<string>> errors)
        {
            throw new NotImplementedException();
        }
    }
}
