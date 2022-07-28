using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public interface IDBColumnMapping<T>
    {
        string ColumnName { get; }
        bool NoInsert { get; }
        bool NoUpdate { get; }
        PropertyInfo ObjectProperty { get; }
        bool Required { get; }
        Func<T, object> PropertyGetter { get; }
        Action<T, IDataRecord, int> GetPropSetterForRecord(Type fieldType);
        Action<T, object> PropertySetter { get; }
    }

    public static class IDBColumnMappingExt
    {
        public static T Parse<T>(this IEnumerable<IDBColumnMapping<T>> mapping, Dictionary<string, object> colNameAndValue) where T : new()
        {
            var result = new T();
            foreach (var c in mapping)
            {
                if (colNameAndValue.TryGetValue(c.ColumnName, out var val))
                {
                    c.PropertySetter(result, val);
                }
            }
            return result;
        }
    }
}