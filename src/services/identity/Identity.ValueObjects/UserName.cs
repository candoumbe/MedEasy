namespace Identity.ValueObjects;

using Identity.ValueObjects.Converters.SystemTextJson;

using System.ComponentModel;
using System.Text.Json.Serialization;

/// <summary>
/// A domain object that wraps usernames
/// </summary>
[JsonConverter(typeof(UserNameJsonConverter))]
[TypeConverter(typeof(UserNameTypeConverter))]
public record UserName
{
    /// <summary>
    /// An empty username
    /// </summary>
    public static UserName Empty => new(string.Empty);

    /// <summary>
    /// Gets the underlying username value;
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserName"/> class.
    /// </summary>
    /// <param name="value"></param>

    private UserName(string value) => Value = value;

    /// <summary>
    /// Wraps <paramref name="input"/> into a <see cref="UserName"/>.
    /// </summary>
    /// <param name="input"></param>
    /// <returns><see cref="UserName"/></returns>
    public static UserName From(string input)
    {
        UserName username = Empty;

        if (!string.IsNullOrWhiteSpace(input))
        {
            username = new(input);
        }

        return username;
    }

    ///<inheritdoc/>
    public override string ToString() => Value;
}
