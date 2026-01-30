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

        /// <summary>
        /// Gets a value indicating whether the object's type would map to a type that would be considered
        /// primitive in JavaScript.  This includes all C# primitive types, string, enum, or decimal.
        /// </summary>
        /// <returns>True if the object maps to a JavaScript primitive type; otherwise, false.</returns>
        public static bool IsJavaScriptPrimitive(this Type t)
        {
            t = UnwrapPrimitive(t);

            return t.IsPrimitive || t == typeof(string) || t == typeof(decimal);
        }

        public static bool IsNullablePrimitive(this Type t)
        {
            var nullable = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
            if(nullable == false && t != typeof(string)) { return false; }

            return t.IsJavaScriptPrimitive();
        }

        public static bool IsNumber(this Type t)
        {
            t = UnwrapPrimitive(t);

            // boolean is the only primitive that is not a number
            if(t == typeof(bool)) { return false; }

            return t.IsPrimitive || t == typeof(decimal);
        }

        public static bool IsUnsignedNumber(this Type t)
        {
            if(t.IsNumber())
            {
                return t == typeof(byte)
                    || t == typeof(uint)
                    || t == typeof(ulong)
                    || t == typeof(ushort)
                    || t == typeof(UIntPtr)
                    || t == typeof(char);
            }

            return false;
        }

        public static bool IsSignedNumber(this Type t)
            => t.IsNumber() && t.IsUnsignedNumber() == false;

        private static Type UnwrapPrimitive(Type t)
        {
            // nullables wrapping primitives are primitives too
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                t = t.GetGenericArguments()[0];
            }
            if(t.IsEnum)
            {
                t = Enum.GetUnderlyingType(t);
            }

            return t;
        }
    }
}
