namespace MedEasy.Abstractions.ValueConverters
{
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// Custom implementation of <see cref="ValueConverterSelector"/> that avoids client-side evaluation when using strongly typed ids.
    /// (see https://andrewlock.net/strongly-typed-ids-in-ef-core-using-strongly-typed-entity-ids-to-avoid-primitive-obsession-part-4/)
    /// </summary>
    public class StronglyTypedIdValueConverterSelector : ValueConverterSelector
    {
        // The dictionary in the base type is private, so we need our own one here.
        private readonly ConcurrentDictionary<(Type ModelClrType, Type ProviderClrType), ValueConverterInfo> _converters
            = new();

        public StronglyTypedIdValueConverterSelector(ValueConverterSelectorDependencies dependencies) : base(dependencies)
        { }

        public override IEnumerable<ValueConverterInfo> Select(Type modelClrType, Type providerClrType = null)
        {
            static Type UnwrapNullableType(Type type) => type is null
                ? null
                : Nullable.GetUnderlyingType(type) ?? type;

            IEnumerable<ValueConverterInfo> baseConverters = base.Select(modelClrType, providerClrType);
            foreach (ValueConverterInfo converter in baseConverters)
            {
                yield return converter;
            }

            // Extract the "real" type T from Nullable<T> if required
            Type underlyingModelType = UnwrapNullableType(modelClrType);
            Type underlyingProviderType = UnwrapNullableType(providerClrType);

            // 'null' means 'get any value converters for the modelClrType'
            if (underlyingProviderType is null || underlyingProviderType == typeof(Guid))
            {
                // Try and get a nested class with the expected name. 
                Type converterType = underlyingModelType.GetNestedType("EfValueConverter");

                if (converterType != null)
                {
                    yield return _converters.GetOrAdd((underlyingModelType, typeof(Guid)),
                                                      _ =>
                                                      {
                                                          // Create an instance of the converter whenever it's requested.
                                                          Func<ValueConverterInfo, ValueConverter> factory = info => (ValueConverter)Activator.CreateInstance(converterType, info.MappingHints);

                                                          // Build the info for our strongly-typed ID => Guid converter
                                                          return new ValueConverterInfo(modelClrType, typeof(Guid), factory);
                                                      }
                    );
                }
            }
        }
    }
}
