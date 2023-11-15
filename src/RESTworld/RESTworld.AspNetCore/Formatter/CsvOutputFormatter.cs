using HAL.Common;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Formatter;

/// <summary>
/// A formatter to output a <see cref="Resource"/>s Embedded["items"] collection as csv.
/// The embedded items must be of type <see cref="Resource{TState}"/> and all have the same state type.
/// </summary>
/// <seealso cref="TextOutputFormatter" />
public class CsvOutputFormatter : TextOutputFormatter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CsvOutputFormatter"/> class.
    /// </summary>
    public CsvOutputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));

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

    /// <inheritdoc/>
    public override IReadOnlyList<string>? GetSupportedContentTypes(string contentType, Type objectType)
    {
        return base.GetSupportedContentTypes(contentType, objectType);
    }

    /// <inheritdoc/>
    protected override bool CanWriteType(Type? type)
    {
        return typeof(Resource).IsAssignableFrom(type);
    }

    /// <inheritdoc/>
    public override bool CanWriteResult(OutputFormatterCanWriteContext context)
    {
        if (!context.ContentType.HasValue || !context.ContentType.Value.Contains("text/csv"))
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

    /// <inheritdoc/>
    public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
    {
        if (context.Object is not Resource resource)
            throw new ArgumentException($"{nameof(context)}.{nameof(context.Object)} is not a {nameof(Resource)}");

        var list = resource.Embedded?[Common.Constants.ListItems];

        // An empty list results in an empty file.
        if (list is null || list.Count == 0)
            return;

        // We know that the elements must be of type Resource.
        // We check that they are of type Resource<T> so we can get the state out of it.
        var firstElement = list.First();
        var elementType = firstElement.GetType();
        if (!elementType.IsGenericType)
            return;

        // The states are the part that should be serialized into the CSV.
        var states = list.Select(r => ((dynamic)r).State);

        var culture = context.HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture;
        var configuration = new CsvHelper.Configuration.CsvConfiguration(culture)
        {
            Encoding = selectedEncoding
        };
        await using var writer = context.WriterFactory(context.HttpContext.Response.Body, selectedEncoding);
        await using var csv = new CsvHelper.CsvWriter(writer, configuration, true);

        await csv.WriteRecordsAsync(states, context.HttpContext.RequestAborted);
    }
}
