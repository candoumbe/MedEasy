#if NET5_0_OR_GREATER
namespace MedEasy.ValueObjects.Converters.SystemTextJson;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts a <see cref="UserName"/> to and from JSON.
/// </summary>
public class PasswordJsonConverter : JsonConverter<Password>
{

    ///<inheritdoc/>
    public override Password Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        return Password.From(reader.GetString());
    }

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, Password value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

#endif