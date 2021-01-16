using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public delegate Task ExternalServiceDelegate(string target,
                                                 string messageName,
                                                 object content,
                                                 string correlationId,
                                                 IReadOnlyDictionary<string, object> parameters);
}
