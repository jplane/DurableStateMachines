﻿using DSM.Common.Model.Actions;
using System.Collections.Generic;

namespace DSM.Common.Model.States
{
    public interface IOnEntryExitMetadata : IModelMetadata
    {
        bool IsEntry { get; }

        IEnumerable<IActionMetadata> GetActions();
    }
}
