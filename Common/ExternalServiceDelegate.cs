using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public delegate Task ExternalServiceDelegate(string correlationId, JObject config);
}
