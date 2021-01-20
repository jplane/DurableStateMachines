using Newtonsoft.Json.Linq;
using StateChartsDotNet.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public delegate Task<string> ExternalQueryDelegate(IQueryConfiguration config);
}
