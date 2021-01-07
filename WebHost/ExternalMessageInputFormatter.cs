using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using StateChartsDotNet.Common.Messages;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.WebHost
{
    public class ExternalMessageInputFormatter : TextInputFormatter
    {
        public ExternalMessageInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanReadType(Type type)
        {
            return typeof(ExternalMessage).IsAssignableFrom(type);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context,
                                                                              Encoding effectiveEncoding)
        {
            Debug.Assert(context != null);
            Debug.Assert(effectiveEncoding != null);

            using var reader = new StreamReader(context.HttpContext.Request.Body, effectiveEncoding);

            try
            {
                var message = JsonConvert.DeserializeObject<ExternalMessage>(await reader.ReadToEndAsync());

                Debug.Assert(message != null);

                return await InputFormatterResult.SuccessAsync(message);
            }
            catch (Exception ex)
            {
                context.ModelState.TryAddModelError(context.ModelName, ex, null);

                return await InputFormatterResult.FailureAsync();
            }
        }
    }
}
