using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System
{
    public static class TypeExtensions
    {
        /// <summary>
          /// Determines whether the <paramref name="genericType"/> is assignable from
          /// <paramref name="givenType"/> taking into account generic definitions
          /// </summary>
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        => givenType != null && genericType != null && givenType == genericType
            || givenType.MapsToGenericTypeDefinition(genericType)
            || givenType.HasInterfaceThatMapsToGenericTypeDefinition(genericType)
            //|| givenType.GetTypeInfo().BaseType.IsAssignableToGenericType(genericType)
            ;


        private static bool HasInterfaceThatMapsToGenericTypeDefinition(this Type givenType, Type genericType)
        => givenType
                .GetTypeInfo()
#if NETSTANDARD1_0 || NETSTANDARD1_1 || NETSTANDARD1_3

                .ImplementedInterfaces
#else
            .GetInterfaces()
#endif
            .Where(it => it.GetTypeInfo().IsGenericType)
                .Any(it => it.GetGenericTypeDefinition() == genericType);


        private static bool MapsToGenericTypeDefinition(this Type givenType, Type genericType)
            => genericType.GetTypeInfo().IsGenericTypeDefinition
            && givenType.GetTypeInfo().IsGenericType
            && givenType.GetGenericTypeDefinition() == genericType;

        /// <summary>
        /// Tests if <paramref name="type"/> is an anonymous type
        /// </summary>
        /// <param name="type">The type under test</param>
        /// <returns><c>true</c>if <paramref name="type"/> is an anonymous type and <c>false</c> otherwise</returns>
        public static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute = type?.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() ?? false;
            bool nameContainsAnonymousType = type?.FullName.Contains("AnonymousType") ?? false;
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }
    }
}
