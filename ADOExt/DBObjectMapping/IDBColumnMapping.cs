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
        DbType DbType { get; }
        bool Required { get; }
        Func<T, object> PropertyGetter { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldType">The Type returned by DataReader for this column.</param>
        /// <returns>Returned setter action does not handle DBNull. Caller must check DBNull before calling this setter action</returns>
        Action<T, IDataRecord, int> GetPropSetterForRecord(Type fieldType);
        Action<T, object> PropertySetter { get; }
    }

    public static class IDBColumnMappingExt
    {
        public static T Parse<T>(this List<IDBColumnMapping<T>> mapping, Dictionary<string, object> colNameAndValue) where T : new()
        {
            var result = new T();
            for (int i = 0; i < mapping.Count; i++)
            {
                var c = mapping[i];
                if (colNameAndValue.TryGetValue(c.ColumnName, out var val))
                {
                    c.PropertySetter(result, val);
                }
            }
            return result;
        }
    }
}