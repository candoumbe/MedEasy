namespace MedEasy.ValueObjects;


using System;
using System.ComponentModel;
using System.Text.RegularExpressions;

#if NET5_0_OR_GREATER
using System.Text.Json.Serialization;
using MedEasy.ValueObjects.Converters.SystemTextJson;
#endif

/// <summary>
/// A domain object that wraps email strings
/// </summary>
#if NET5_0_OR_GREATER
[JsonConverter(typeof(EmailJsonConverter))] 
#endif
[TypeConverter(typeof(EmailTypeConverter))]
public record Email
{
    /// <summary>
    /// Email pattern
    /// </summary>
    public const string EmailPattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
#if !NET7_0_OR_GREATER
    private static readonly Regex EmailRegex = new(EmailPattern,
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
