using Newtonsoft.Json.Linq;
using DSM.Common.Model.Execution;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSM.Common
{
    public delegate Task ExternalServiceDelegate(string correlationId, object config);
}
