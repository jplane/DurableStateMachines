using CoreEngine.Abstractions.Model.States.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model
{
    public interface IModelMetadata
    {
        Task<IRootStateMetadata> GetRootState();
    }
}
