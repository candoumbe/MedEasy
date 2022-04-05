namespace Identity.ValueObjects.Converters.SystemTextJson;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
///    Converts a <see cref="Email" /> to and from JSON.
/// </summary>
public class EmailJsonConverter : JsonConverter<Email>
{
    ///<inheritdoc/>
    public override Email Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        return Email.From(reader.GetString());
    }

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Email value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}