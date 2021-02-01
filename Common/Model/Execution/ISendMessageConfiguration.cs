﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DSM.Common.Model.Execution
{
    public interface ISendMessageConfiguration
    {
        void ResolveConfigValues(Func<string, string> resolver);
    }
}
