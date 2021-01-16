using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public delegate Task<string> ExternalQueryDelegate(string target,
                                                       IReadOnlyDictionary<string, object> parameters);
}
