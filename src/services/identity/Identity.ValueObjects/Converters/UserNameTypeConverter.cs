namespace Identity.ValueObjects;

using System;
using System.ComponentModel;
using System.Globalization;

public class UserNameTypeConverter : TypeConverter
{
    ///<inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    ///<inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

    ///<inheritdoc/>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => UserName.From(value as string ?? string.Empty);

}