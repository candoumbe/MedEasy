namespace MedEasy.ValueObjects;
using System.Globalization;

/// <summary>
/// This class is used to convert a <see cref="string"/> to a <see cref="UserName"/> object and vice-versa.
/// </summary>
public class UserNameTypeConverter : GenericTypeConverter<string, UserName>
{
    ///<inheritdoc/>
    protected override UserName From(string value, CultureInfo culture) => UserName.From(value);

    ///<inheritdoc/>
    protected override string To(UserName value, CultureInfo culture) => value.Value;
}