using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Health
{
    /// <summary>
    /// Writes health check responses as valid HAL resources.
    /// </summary>
    public static class HealthCheckHALResponseWriter
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        static HealthCheckHALResponseWriter()
        {
            _jsonSerializerOptions.Converters.Add(new JsonTimeSpanConverter());
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter(JsonNamingPolicy.CamelCase));
        }

        /// <summary>
        /// Writes the health check response as a HAL resources.
        /// </summary>
        public static Task WriteResponseAsync(HttpContext context, HealthReport report)
        {
            var resource = new Resource<HealthReport> { State = report }.AddSelfLink(context.Request.GetEncodedUrl());
            return context.Response.WriteAsJsonAsync(resource, _jsonSerializerOptions);
        }

        private class JsonTimeSpanConverter : JsonConverter<TimeSpan>
        {
            public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                => TimeSpan.Parse(reader.GetString());

            public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString());
        }
    }
}