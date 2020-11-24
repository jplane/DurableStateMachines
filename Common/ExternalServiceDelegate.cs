using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.Common
{
    public delegate Task ExternalServiceDelegate(string target,
                                                 string messageName,
                                                 object content,
                                                 IReadOnlyDictionary<string, object> parameters);
}
