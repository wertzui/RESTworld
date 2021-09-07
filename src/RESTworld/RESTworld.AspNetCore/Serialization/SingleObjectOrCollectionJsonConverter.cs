using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RESTworld.AspNetCore.Serialization
{
    /// <summary>
    /// A converter which accepts either a single object, or a JSON array and deserializes (or serializes) it.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    public class SingleObjectOrCollectionJsonConverter<T> : JsonConverter<SingleObjectOrCollection<T>>
    {
        /// <inheritdoc/>
        public override SingleObjectOrCollection<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                return new(JsonSerializer.Deserialize<T>(ref reader, options));
            }
            else if (reader.TokenType == JsonTokenType.StartArray)
            {
                return new(JsonSerializer.Deserialize<IReadOnlyCollection<T>>(ref reader, options));
            }

            throw new JsonException("The value must either be an object or an array.", null, null, reader.BytesConsumed);
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SingleObjectOrCollection<T> value, JsonSerializerOptions options)
        {
            if (value.ContainsCollection)
            {
                JsonSerializer.Serialize(writer, value.Collection, options);
            }
            else
            {
                JsonSerializer.Serialize(writer, value.SingleObject, options);
            }
        }
    }
}