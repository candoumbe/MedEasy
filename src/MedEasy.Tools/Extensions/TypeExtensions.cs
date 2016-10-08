using System;
using System.Linq;
using System.Reflection;
namespace MedEasy.Tools.Extensions
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
                .GetInterfaces()
                .Where(it => it.GetTypeInfo().IsGenericType)
                .Any(it => it.GetGenericTypeDefinition() == genericType);
           

        private static bool MapsToGenericTypeDefinition(this Type givenType, Type genericType)
            => genericType.GetTypeInfo().IsGenericTypeDefinition
            && givenType.GetTypeInfo().IsGenericType
            && givenType.GetGenericTypeDefinition() == genericType;

    }
}
