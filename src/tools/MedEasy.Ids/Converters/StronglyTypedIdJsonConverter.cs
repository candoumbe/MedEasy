namespace MedEasy.Ids.Converters
{
    using System;
    using System.Collections.Concurrent;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Converts a <typeparamref name="TStronglyTypedId"/> value from/to JSON
    /// </summary>
    /// <typeparam name="TStronglyTypedId">Type of value to convert from/to</typeparam>
    /// <typeparam name="TValue">Type of the raw value wrapped inside <see cref="StronglyTypedId{TValue}"/></typeparam>
    public class StronglyTypedIdJsonConverter<TStronglyTypedId, TValue> : JsonConverter<TStronglyTypedId>
        where TStronglyTypedId : StronglyTypedId<TValue>
        where TValue : notnull
    {
        ///<inheritdoc/>
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
