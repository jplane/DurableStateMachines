using CoreEngine.Abstractions.Model.Execution.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IOnEntryExitMetadata
    {
        bool IsEntry { get; }

        Task<IEnumerable<IExecutableContentMetadata>> GetExecutableContent();
    }
}
