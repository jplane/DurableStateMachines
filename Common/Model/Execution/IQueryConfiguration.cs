using System;
using System.Collections.Generic;
using System.Text;

namespace StateChartsDotNet.Common.Model.Execution
{
    public interface IQueryConfiguration
    {
        void ResolveConfigValues(Func<string, string> resolver);
    }
}
