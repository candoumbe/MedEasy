using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;

namespace MedEasy.Ids.Converters
{
    public class StronglyTypedIdConverter<TValue> : TypeConverter where TValue : notnull
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
        public StronglyTypedIdConverter(Type type)
        {
            _type = type;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string)
                || sourceType == typeof(TValue)
                || base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string)
                || destinationType == typeof(TValue)
                || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string s)
            {
                value = IdValueConverter.ConvertFrom(s);
            }

            object result;

            if (value is TValue idValue)
            {
                Func<TValue, object> factory = StronglyTypedIdHelper.GetFactory<TValue>(_type);
                result = factory(idValue);
            }
            else
            {
                result = base.ConvertFrom(context, culture, value);
            }

            return result;
        }

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

    public class StronglyTypedIdConverter : TypeConverter
    {
        private static readonly ConcurrentDictionary<Type, TypeConverter> ActualConverters = new();

        private readonly TypeConverter _innerConverter;

        public StronglyTypedIdConverter(Type stronglyTypedIdType)
        {
            _innerConverter = ActualConverters.GetOrAdd(stronglyTypedIdType, CreateActualConverter);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
            _innerConverter.CanConvertFrom(context, sourceType);
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
            _innerConverter.CanConvertTo(context, destinationType);
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
            _innerConverter.ConvertFrom(context, culture, value);
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
            _innerConverter.ConvertTo(context, culture, value, destinationType);

        private static TypeConverter CreateActualConverter(Type stronglyTypedIdType)
        {
            if (!StronglyTypedIdHelper.IsStronglyTypedId(stronglyTypedIdType, out Type idType))
            {
                throw new InvalidOperationException($"The type '{stronglyTypedIdType}' is not a strongly typed id");
            }

            Type actualConverterType = typeof(StronglyTypedIdConverter<>).MakeGenericType(idType);
            return (TypeConverter)Activator.CreateInstance(actualConverterType, stronglyTypedIdType)!;
        }
    }
}
