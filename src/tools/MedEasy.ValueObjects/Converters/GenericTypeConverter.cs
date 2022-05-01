namespace MedEasy.ValueObjects;

using System;
using System.ComponentModel;
using System.Globalization;

/// <summary>
/// A generic converter that can convert from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>
/// </summary>
/// <typeparam name="TFrom">Source type of the converter</typeparam>
/// <typeparam name="TTo">Destination type of the converter</typeparam>
public abstract class GenericTypeConverter<TFrom, TTo> : TypeConverter
    where TFrom: class
    where TTo: class
{
    ///<inheritdoc/>
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        => sourceType == typeof(TFrom) || base.CanConvertFrom(context, sourceType);

    ///<inheritdoc/>
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        => destinationType == typeof(TTo) || base.CanConvertTo(context, destinationType);

    ///<inheritdoc/>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => From(value as TFrom ?? default, culture);

    ///<inheritdoc/>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        return destinationType == typeof(TTo)
            ? To(value as TTo ?? default, culture)
            : base.ConvertTo(context, culture, value, destinationType);
    }


    /// <summary>
    /// Converts <see cref="TFrom"/> value to a <see cref="TTo"/> value.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns></returns>
    /// <remarks>
    /// This method is called by <see cref="ConvertFrom(ITypeDescriptorContext, CultureInfo, object)"/> when the source type is <typeparamref name="TFrom"/>.
    /// </remarks>
    protected abstract TTo From(TFrom value, CultureInfo culture);

    /// <summary>
    /// Converts <see cref="TTo"/> value to a <see cref="TFrom"/> value.
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <returns></returns>
    /// <remarks>This method is called by <see cref="ConvertTo(ITypeDescriptorContext, CultureInfo, object, Type)"/> when the destination type is <typeparamref name="TFrom"/></remarks>
    protected abstract TFrom To(TTo value, CultureInfo culture);
}