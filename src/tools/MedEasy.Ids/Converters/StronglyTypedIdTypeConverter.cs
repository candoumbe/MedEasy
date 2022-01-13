namespace MedEasy.Ids.Converters
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Globalization;

    /// <summary>
    /// Provides an unified way to convert from/to <see cref="StronglyTypedId{TValue}"/>.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class StronglyTypedIdTypeConverter<TValue> : TypeConverter where TValue : notnull
    {
        private static readonly TypeConverter IdValueConverter = GetIdValueConverter();

        private static TypeConverter GetIdValueConverter()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(TValue));
            if (!converter.CanConvertFrom(typeof(string)))
            {
                throw new InvalidOperationException(
                    $"Type '{typeof(TValue)}' doesn't have a converter that can convert from string");
            }

            return converter;
        }

        private readonly Type _type;

        /// <summary>
        /// Builds a new <see cref="StronglyTypedIdTypeConverter"/> instance with the specified <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        public StronglyTypedIdTypeConverter(Type type)
        {
            _type = type;
        }

        ///<inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string)
                || sourceType == typeof(TValue)
                || base.CanConvertFrom(context, sourceType);
        }

        ///<inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string)
                || destinationType == typeof(TValue)
                || base.CanConvertTo(context, destinationType);
        }

        ///<inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                value = IdValueConverter.ConvertFrom(s);
            }

            object result;

            switch (value)
            {
                case TValue idValue:
                    {
                        Func<TValue, object> factory = StronglyTypedIdHelper.GetFactory<TValue>(_type);
                        result = factory(idValue);
                        break;
                    }

                default:
                    result = base.ConvertFrom(context, culture, value);
                    break;
            }

            return result;
        }

        ///<inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            StronglyTypedId<TValue> stronglyTypedId = (StronglyTypedId<TValue>)value;
            TValue idValue = stronglyTypedId.Value;

            object result;

            if (destinationType == typeof(string))
            {
                result = idValue.ToString()!;
            }
            else if (destinationType == typeof(TValue))
            {
                result = idValue;
            }
            else
            {
                result = base.ConvertTo(context, culture, value, destinationType);
            }

            return result;
        }
    }

    /// <summary>
    /// Provides a unified way to convert from and to a <see cref="StronglyTypedId{TValue}"/>
    /// </summary>
    public class StronglyTypedIdTypeConverter : TypeConverter
    {
        private static readonly ConcurrentDictionary<Type, TypeConverter> ActualConverters = new();

        private readonly TypeConverter _innerConverter;

        /// <summary>
        /// Builds a new <see cref="StronglyTypedIdTypeConverter"/> instance that can handle the specified
        /// <paramref name="stronglyTypedIdType"/>.
        /// </summary>
        /// <param name="stronglyTypedIdType"></param>
        public StronglyTypedIdTypeConverter(Type stronglyTypedIdType)
        {
            _innerConverter = ActualConverters.GetOrAdd(stronglyTypedIdType, CreateActualConverter);
        }

        ///<inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            => _innerConverter.CanConvertFrom(context, sourceType);

        ///<inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            => _innerConverter.CanConvertTo(context, destinationType);

        ///<inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => _innerConverter.ConvertFrom(context, culture, value);

        ///<inheritdoc/>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            => _innerConverter.ConvertTo(context, culture, value, destinationType);

        private static TypeConverter CreateActualConverter(Type stronglyTypedIdType)
        {
            if (!StronglyTypedIdHelper.TryIsStronglyTypedId(stronglyTypedIdType, out Type idType))
            {
                throw new InvalidOperationException($"The type '{stronglyTypedIdType}' is not a strongly typed id");
            }

            Type actualConverterType = typeof(StronglyTypedIdTypeConverter<>).MakeGenericType(idType);
            return (TypeConverter)Activator.CreateInstance(actualConverterType, stronglyTypedIdType)!;
        }
    }
}
