namespace Identity.ValueObjects;

using System;
using System.ComponentModel;
using System.Globalization;

/// <summary>
/// <see cref="TypeConverter"/> that handles converting <see cref="Email"/> from and to <see cref="string"/>.
/// </summary>
internal class EmailTypeConverter : TypeConverter
{
    ///<inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    ///<inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        => base.ConvertTo(context, culture, value, destinationType);

    ///<inheritdoc/>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => Email.From(value as string ?? string.Empty);
}
