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
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        static HealthCheckHALResponseWriter()
        {
            _jsonSerializerOptions.Converters.Add(new JsonTimeSpanConverter());
            _jsonSerializerOptions.Converters.Add(new JsonExceptionConverter());
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
            {
                var timeSpanString = reader.GetString();
                if (!TimeSpan.TryParse(timeSpanString, out var timeSpan))
                    throw new JsonException($"The value {timeSpanString} is not a valid TimeSpan.");
                return timeSpan;
            }

            public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString());
        }

        /// <summary>
        /// <see cref="HealthCheckResult"/> may contain an exception.
        /// Without this converter, the serialization of that exception would throw a <see cref="NotSupportedException"/> from System.Text.Json.
        /// </summary>
        private class JsonExceptionConverter : JsonConverter<Exception>
        {
            public override Exception? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotSupportedException();
            }

            public override void Write(Utf8JsonWriter writer, Exception value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();
                writer.WriteString(nameof(Exception.Message), value.Message);
                writer.WriteString(nameof(Exception.StackTrace), value.StackTrace);
                if (value.Data is not null)
                {
                    writer.WritePropertyName(nameof(Exception.Data));
                    JsonSerializer.Serialize(writer, value.Data, options);
                }
                if (value.InnerException is not null)
                {
                    writer.WritePropertyName(nameof(Exception.InnerException));
                    JsonSerializer.Serialize(writer, value.InnerException, options);
                }
                writer.WriteEndObject();
            }
        }
    }
}