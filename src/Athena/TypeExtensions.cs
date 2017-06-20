using System;
using System.Collections.Generic;
using System.Reflection;

namespace Athena
{
    public static class TypeExtensions
    {
        public static IEnumerable<Type> GetParentTypesFor(this Type type, bool includeInterfaces = true)
        {
            if (type == null)
                yield break;

            yield return type;

            var baseType = type.GetTypeInfo().BaseType;

            while (baseType != null)
            {
                yield return baseType;
                baseType = baseType.GetTypeInfo().BaseType;
            }
            
            if(!includeInterfaces)
                yield break;

            foreach (var @interface in type.GetInterfaces())
                yield return @interface;
        }
    }
}