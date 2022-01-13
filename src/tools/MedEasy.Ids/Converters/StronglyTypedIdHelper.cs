namespace MedEasy.Ids.Converters
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    /// <summary>
    /// Helper class
    /// </summary>
    internal static class StronglyTypedIdHelper
    {
        private static readonly ConcurrentDictionary<Type, Delegate> StronglyTypedIdFactories = new();

        public static Func<TValue, object> GetFactory<TValue>(Type stronglyTypedIdType)
            where TValue : notnull
        {
            return (Func<TValue, object>)StronglyTypedIdFactories.GetOrAdd(
                stronglyTypedIdType,
                CreateFactory<TValue>);
        }

        private static Func<TValue, object> CreateFactory<TValue>(Type stronglyTypedIdType)
            where TValue : notnull
        {
            if (!IsStronglyTypedId(stronglyTypedIdType))
            {
                throw new ArgumentException($"Type '{stronglyTypedIdType}' is not a strongly-typed id type", nameof(stronglyTypedIdType));
            }

            System.Reflection.ConstructorInfo ctor = stronglyTypedIdType.GetConstructor(new[] { typeof(TValue) });
            if (ctor is null)
            {
                throw new ArgumentException($"Type '{stronglyTypedIdType}' doesn't have a constructor with one parameter of type '{typeof(TValue)}'", nameof(stronglyTypedIdType));
            }

            ParameterExpression param = Expression.Parameter(typeof(TValue), "value");
            NewExpression body = Expression.New(ctor, param);
            Expression<Func<TValue, object>> lambda = Expression.Lambda<Func<TValue, object>>(body, param);

            return lambda.Compile();
        }

        /// <summary>
        /// Tests if <paramref name="type"/> stands for a Stronglu
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsStronglyTypedId(Type type) => TryIsStronglyTypedId(type, out _);

        /// <summary>
        /// Checks if the specified <paramref name="type"/> is a <see cref="StronglyTypedId{TValue}"/>
        /// </summary>
        /// <param name="type">The type to test</param>
        /// <param name="idType">The raw</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
#if NETSTANDARD2_0 
        public static bool TryIsStronglyTypedId(Type type, out Type idType)
#else
        public static bool TryIsStronglyTypedId(Type type, [NotNullWhen(true)] out Type idType)
#endif
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            bool isStronglyType = false;
            if (type.BaseType is Type baseType &&
                baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
            {
                idType = baseType.GetGenericArguments()[0];
                isStronglyType = true;
            }
            else if (type.BaseType == typeof(StronglyTypedGuidId))
            {
                idType = typeof(Guid);
                isStronglyType = true;
            }
            else
            {
                idType = null;
            }

            return isStronglyType;
        }
    }
}
