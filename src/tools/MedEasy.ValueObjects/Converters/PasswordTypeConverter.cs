namespace MedEasy.ValueObjects;
using System.Globalization;

/// <summary>
/// This class is used to convert a <see cref="string"/> to a <see cref="Email"/> object and vice-versa.
/// </summary>
public class PasswordTypeConverter : GenericTypeConverter<string, Password>
{
    ///<inheritdoc/>
    protected override Password From(string value, CultureInfo culture) => Password.From(value);

    ///<inheritdoc/>
    protected override string To(Password value, CultureInfo culture) => value.Value;
}