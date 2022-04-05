namespace Identity.ValueObjects.Converters.SystemTextJson;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Converts a <see cref="UserName"/> to and from JSON.
/// </summary>
public class UserNameJsonConverter : JsonConverter<UserName>
{

    ///<inheritdoc/>
    public override UserName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        return UserName.From(reader.GetString());
    }

    ///<inheritdoc/>
    public override void Write(Utf8JsonWriter writer, UserName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}
