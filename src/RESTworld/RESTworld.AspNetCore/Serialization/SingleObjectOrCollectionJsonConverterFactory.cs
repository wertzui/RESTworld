using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RESTworld.AspNetCore.Serialization
{
    /// <summary>
    /// A factory to create a <see cref="SingleObjectOrCollectionJsonConverter{T}"/> with the correct generic type.
    /// </summary>
    public class SingleObjectOrCollectionJsonConverterFactory : JsonConverterFactory
    {
        /// <inheritdoc/>
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(SingleObjectOrCollection<>))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var objectType = typeToConvert.GetGenericArguments()[0];

            var converter = (JsonConverter)Activator.CreateInstance(typeof(SingleObjectOrCollectionJsonConverter<>).MakeGenericType(objectType));

            return converter;
        }
    }
}
