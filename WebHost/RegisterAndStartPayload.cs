using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateChartsDotNet.WebHost
{
    public class RegisterAndStartPayload
    {
        public string MetadataId { get; set; }
        public string InstanceId { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public IStateChartMetadata Metadata { get; set; }
    }

    public class RegisterAndStartPayloadInputFormatter : TextInputFormatter
    {
        public RegisterAndStartPayloadInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(RegisterAndStartPayload);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context,
                                                                              Encoding effectiveEncoding)
        {
            Debug.Assert(context != null);
            Debug.Assert(effectiveEncoding != null);

            try
            {
                var headers = new RegisterAndStartPayload();

                headers.MetadataId = context.HttpContext.Request.Headers["X-SCDN-METADATA-ID"].ToString();

                headers.InstanceId = context.HttpContext.Request.Headers["X-SCDN-INSTANCE-ID"].ToString();

                var parameters = context.HttpContext.Request.Headers.Where(h => h.Key.StartsWith("X-SCDN-PARAM-"))
                                                                    .ToDictionary(h => h.Key[13..],
                                                                                  h => JsonConvert.DeserializeObject(h.Value.ToString()));

                headers.Parameters = parameters;

                headers.Metadata = await StateChartInputFormatter.GetMetadataAsync(context.HttpContext, effectiveEncoding);

                return await InputFormatterResult.SuccessAsync(headers);
            }
            catch (Exception ex)
            {
                context.ModelState.TryAddModelError(context.ModelName, ex, null);

                return await InputFormatterResult.FailureAsync();
            }
        }
    }
}
