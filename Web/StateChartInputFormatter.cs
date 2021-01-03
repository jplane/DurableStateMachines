using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using StateChartsDotNet.Common.Model.States;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StateChartsDotNet.Web
{
    public class StateChartInputFormatter : TextInputFormatter
    {
        public StateChartInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/plain"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/json"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/xml"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(IStateChartMetadata);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context,
                                                                              Encoding effectiveEncoding)
        {
            Debug.Assert(context != null);
            Debug.Assert(effectiveEncoding != null);

            try
            {
                var metadata = await GetMetadataAsync(context.HttpContext, effectiveEncoding);

                Debug.Assert(metadata != null);

                return await InputFormatterResult.SuccessAsync(metadata);
            }
            catch (Exception ex)
            {
                context.ModelState.TryAddModelError(context.ModelName, ex, null);

                return await InputFormatterResult.FailureAsync();
            }
        }

        internal static async Task<IStateChartMetadata> GetMetadataAsync(HttpContext context, Encoding effectiveEncoding)
        {
            using var reader = new StreamReader(context.Request.Body, effectiveEncoding);

            Func<string, CancellationToken, Task<IStateChartMetadata>> factory = null;

            switch (context.Request.ContentType)
            {
                case "application/json":
                    factory = Metadata.Json.States.StateChart.FromStringAsync;
                    break;

                case "application/xml":
                    factory = Metadata.Xml.States.StateChart.FromStringAsync;
                    break;

                case "text/plain":
                    factory = Metadata.Fluent.States.StateChart.FromStringAsync;
                    break;
            }

            Debug.Assert(factory != null);

            return await factory(await reader.ReadToEndAsync(), CancellationToken.None);
        }
    }
}
