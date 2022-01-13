namespace MedEasy.Ids.Converters
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Supports converting <see cref="StronglyTypedId{TValue}"/> using a factory pattern.
    /// This is a implementation of <see cref="JsonConverterFactory"/>.
    /// </summary>
    public class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

        ///<inheritdoc/>
        public override bool CanConvert(Type typeToConvert) => StronglyTypedIdHelper.IsStronglyTypedId(typeToConvert);

        ///<inheritdoc/>
        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return Cache.GetOrAdd(typeToConvert, CreateConverter);
        }

        private static JsonConverter CreateConverter(Type typeToConvert)
        {
            if (!StronglyTypedIdHelper.TryIsStronglyTypedId(typeToConvert, out var valueType))
            {
                throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");
            }

            var type = typeof(StronglyTypedIdJsonConverter<,>).MakeGenericType(typeToConvert, valueType);
            return (JsonConverter)Activator.CreateInstance(type);
        }
    }
}
