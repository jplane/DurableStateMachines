﻿using CoreEngine.Abstractions.Model.DataManipulation.Metadata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine.Abstractions.Model.States.Metadata
{
    public interface IFinalStateMetadata : IStateMetadata
    {
        Task<IDonedataMetadata> GetDonedata();
    }
}
