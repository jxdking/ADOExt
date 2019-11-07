using System;

namespace MagicEastern.ADOExt
{
    internal static class TypeExt
    {
        public static Type UnwrapIfNullable(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return type.GetGenericArguments()[0];
            }
            return type;
        }
    }
}