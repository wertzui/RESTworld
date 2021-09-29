using HAL.Common;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Formatter
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));

            // I tried to add these explicitly, but it does not change anything.
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv; charset=utf-8"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv; charset=utf-8; v=1.0"));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv; charset=utf-8; v=2.0"));

            // UTF-8 is default, but all are supported if requested.
            foreach (var encodingInfo in Encoding.GetEncodings())
            {
                var encoding = encodingInfo.GetEncoding();
                if (encoding == Encoding.UTF8)
                    SupportedEncodings.Insert(0, encoding);
                else
                    SupportedEncodings.Add(encoding);
            }
        }

        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, System.Type objectType)
        {
            // This method only gets called during startup with either "application/hal+json" or "text/csv".
            // On my Controller method I have the attribute [Produces("application/hal+json", "text/csv")]
            return base.GetSupportedContentTypes(contentType, objectType);
        }

        protected override bool CanWriteType(System.Type type)
        {
            // This method only gets called if the Accept header is exactly "text/csv".
            return typeof(Resource).IsAssignableFrom(type);
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            // This method only gets called if the Accept header is exactly "text/csv" if the formatter is at the end of the list.
            if (!context.ContentType.Value.Contains("text/csv"))
                return false;

            var resource = context.Object as Resource;
            if (resource == null)
                return false;

            if (resource.Embedded == null)
                return false;

            if (!resource.Embedded.ContainsKey(Common.Constants.ListItems))
                return false;

            return true;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            // This method only gets called if the Accept header is exactly "text/csv" if the formatter is at the end of the list.
            var resource = (Resource)context.Object;
            var list = resource.Embedded[Common.Constants.ListItems];

            // An empty list results in an empty file.
            if (list == null || list.Count == 0)
                return;

            // We know that the element must be of type Resource.
            // We check that is of type Resource<T> so we can get the state out of it.
            var firstElement = list.First();
            var elementType = firstElement.GetType();
            if (!elementType.IsGenericType)
                return;

            //var stateType = elementType.GetGenericArguments()[0];
            // The states are the part that should be serialized into the CSV.
            var states = list.Select(r => ((dynamic)r).State);

            var culture = context.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture;
            var configuration = new CsvHelper.Configuration.CsvConfiguration(culture)
            {
                Encoding = selectedEncoding,
                LeaveOpen = true,
            };
            await using var writer = context.WriterFactory(context.HttpContext.Response.Body, selectedEncoding);
            await using var csv = new CsvHelper.CsvWriter(writer, configuration);

            await csv.WriteRecordsAsync(states, context.HttpContext.RequestAborted);
        }
    }
}
