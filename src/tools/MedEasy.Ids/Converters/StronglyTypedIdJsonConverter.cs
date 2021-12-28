namespace MedEasy.Ids.Converters
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class StronglyTypedIdJsonConverterFactory : JsonConverterFactory
    {
        private static readonly ConcurrentDictionary<Type, JsonConverter> Cache = new();

        public override bool CanConvert(Type typeToConvert)
        {
            return StronglyTypedIdHelper.IsStronglyTypedId(typeToConvert);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return Cache.GetOrAdd(typeToConvert, CreateConverter);
        }

        private static JsonConverter CreateConverter(Type typeToConvert)
        {
            if (!StronglyTypedIdHelper.IsStronglyTypedId(typeToConvert, out var valueType))
            {
                throw new InvalidOperationException($"Cannot create converter for '{typeToConvert}'");
            }

            var type = typeof(StronglyTypedIdJsonConverter<,>).MakeGenericType(typeToConvert, valueType);
            return (JsonConverter)Activator.CreateInstance(type);
        }
    }

    public class StronglyTypedIdJsonConverter<TStronglyTypedId, TValue> : JsonConverter<TStronglyTypedId>
        where TStronglyTypedId : StronglyTypedId<TValue>
        where TValue : notnull
    {
        public override TStronglyTypedId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            TStronglyTypedId stronglyTypedId = null;

            if (reader.TokenType is not JsonTokenType.Null)
            {
                var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                var factory = StronglyTypedIdHelper.GetFactory<TValue>(typeToConvert);
                stronglyTypedId =  (TStronglyTypedId)factory(value);
            }
            return stronglyTypedId;
        }

        ///<inheritdoc/>
        public override void Write(Utf8JsonWriter writer, TStronglyTypedId value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                if (options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull)
                {
                    writer.WriteNullValue();
                }
            }
            else
            {
                JsonSerializer.Serialize(writer, value.Value, options);
            }
        }
    }
}
