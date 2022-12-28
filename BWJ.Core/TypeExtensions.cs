using System;

namespace BWJ.Core
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Determines whether or not a given type is anonymous.
        /// Note that this method is heuristic.  While this method can be used to definitively determine that a type is not anonymous,
        /// practically negligible edge cases where this method could return a false negative do exist.
        /// </summary>
        /// <param name="obj">
        /// Type to evaluate
        /// </param>
        /// <returns>True if this type is not anonymous</returns>
        public static bool IsNotAnonymous(this Type type)
            => (type.Name.StartsWith("<>") && type.Name.Contains("AnonymousType")) == false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="genericType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <see cref="https://stackoverflow.com/questions/457676/check-if-a-class-is-derived-from-a-generic-class"/>
        public static bool IsSubclassOfGenericClassDefinition(this Type type, Type genericType)
        {
            MethodGuard.NoNull(new { genericType, type });
            MethodGuard.Acceptable<Type>(
                new { genericType },
                t => t!.IsClass && t.IsGenericTypeDefinition,
                "Argument must represent a generic type definition");

            // to ensure behavior similar to Type.IsSubclassOf
            // see https://docs.microsoft.com/en-us/dotnet/api/system.type.issubclassof?view=net-5.0
            if (type.IsClass && type != genericType)
            {
                type = type.BaseType!;
            }
            else
            {
                return false;
            }

            while (type is not null && type != typeof(object))
            {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (genericType == cur)
                {
                    return true;
                }

                type = type.BaseType!;
            }
            return false;
        }
    }
}
