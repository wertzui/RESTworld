using HAL.AspNetCore.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Health
{
    public static class HealthCheckHALResponseWriter
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        static HealthCheckHALResponseWriter()
        {
            _jsonSerializerOptions.Converters.Add(new JsonTimeSpanConverter());
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

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