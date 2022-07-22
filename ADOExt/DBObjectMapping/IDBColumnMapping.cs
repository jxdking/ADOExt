using System;
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
}