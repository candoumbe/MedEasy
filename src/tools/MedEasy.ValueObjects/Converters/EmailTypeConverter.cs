namespace MedEasy.ValueObjects;
using System.Globalization;

/// <summary>
/// This class is used to convert a <see cref="string"/> to a <see cref="Email"/> object and vice-versa.
/// </summary>
public class EmailTypeConverter : GenericTypeConverter<string, Email>
{
    ///<inheritdoc/>
    protected override Email From(string value, CultureInfo culture) => Email.From(value);

    ///<inheritdoc/>
    protected override string To(Email value, CultureInfo culture) => value.Value;
}