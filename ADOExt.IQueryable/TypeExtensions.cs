using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt.IQueryable
{
    public static class TypeExtensions
    {
        public static Type GetSequenceElementType(this Type type)
        {
            var enumerableType = type.GetInterfaces()
                .Concat(new[] { type })
                .FirstOrDefault(t => t.IsGenericType &&
                                      t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableType?.GetGenericArguments()[0] ?? type;
        }
    }
}
