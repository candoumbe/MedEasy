namespace Identity.ValueObjects;

using Identity.ValueObjects.Converters.SystemTextJson;

using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

/// <summary>
/// A domain object that wraps email strings
/// </summary>
[JsonConverter(typeof(EmailJsonConverter))]
[TypeConverter(typeof(EmailTypeConverter))]
public record Email
{

#if !NET7_0_OR_GREATER
    private static readonly Regex EmailRegex = new(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?",
                                               RegexOptions.IgnoreCase,
                                               TimeSpan.FromSeconds(1));
#else
    [RegexGenerator(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?",
                    RegexOptions.IgnoreCase,
                    1000)]
    private static readonly Regex EmailRegex;
#endif
    /// <summary>
    /// An empty username
    /// </summary>
    public static Email Empty => new(string.Empty);

    /// <summary>
    /// Gets the underlying username value;
    /// </summary>
    public string Value { get; }

    private Email(string value) => Value = value;

    /// <summary>
    /// Wraps <paramref name="input"/> intot a 
    /// </summary>
    /// <param name="input"></param>
    /// <returns>a valid <see cref="Email"/></returns>
    /// <exception cref="InvalidOperationException">if <paramref name="input"/> is not a empty string and is not a well formed email adress.</exception>
    public static Email From(string input)
    {
        Email email;

        if (string.IsNullOrWhiteSpace(input) )
        {
            email = Empty;
        }
        else
        {
            email = EmailRegex.IsMatch(input)
                ? new(input)
                : throw new InvalidOperationException($"{nameof(input)} is not a valid email adress");
        }

        return email;
    }

    ///<inheritdoc/>
    public override string ToString() => Value;
}
