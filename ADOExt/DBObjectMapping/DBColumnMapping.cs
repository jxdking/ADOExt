using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBColumnMapping<T> : IDBColumnMapping<T>
    {
        #region IDBColumnMapping
        public string ColumnName { get; protected set; }
        public bool NoInsert { get; protected set; }
        public bool NoUpdate { get; protected set; }
        /// <summary>
        /// for value type column, the Required will always be true.
        /// </summary>
        public bool Required { get; protected set; }
        public PropertyInfo ObjectProperty { get; protected set; }
        public DbType DbType { get; protected set; }
        public Func<T, object> PropertyGetter { get; protected set; }
        public Action<T, object> PropertySetter { get; protected set; }

        private readonly ConcurrentDictionary<Type, Action<T, IDataRecord, int>> _CachePropReaderSetters =
            new ConcurrentDictionary<Type, Action<T, IDataRecord, int>>();


        public Action<T, IDataRecord, int> GetPropSetterForRecord(Type fieldType)
        {
            return _CachePropReaderSetters.GetOrAdd(fieldType, _ => CreatePropertyReaderSetter(fieldType));
        }
        #endregion
        protected DBColumnMapping()
        {

        }

        public override string ToString()
        {
            return ColumnName;
        }

        public DBColumnMapping(PropertyInfo objectProperty, string columnName, bool required = false, bool noInsert = false, bool noUpdate = false)
        {
            var t = objectProperty.PropertyType;
            Required = required || Nullable.GetUnderlyingType(t) == null && t.IsValueType;
            ObjectProperty = objectProperty;
            DbType = objectProperty.PropertyType.ToDbType();
            ColumnName = columnName;
            NoInsert = noInsert;
            NoUpdate = noUpdate;
            PropertySetter = CreatePropertyObjectSetter(objectProperty);
            PropertyGetter = CreatePropertyObjectGetter(objectProperty);
        }

        private BinaryExpression AssignExp(PropertyInfo pi, Expression objParam, Expression valParam)
        {
            var propExp = Expression.Property(objParam, pi);
            var assExp = Expression.Assign(propExp, valParam);
            return assExp;
        }

        protected Action<T, object> CreatePropertyObjectSetter(PropertyInfo pi)
        {
            var pObj = Expression.Parameter(typeof(T), "obj");
            var pVal = Expression.Parameter(typeof(object), "val");
            var castedVal = Expression.Convert(pVal, pi.PropertyType);
            var assign = AssignExp(pi, pObj, castedVal);
            return Expression.Lambda<Action<T, object>>(assign, pObj, pVal).Compile();
        }

        private Func<T, Object> CreatePropertyObjectGetter(PropertyInfo pi)
        {
            var objParam = Expression.Parameter(typeof(T), "obj");
            var propExp = Expression.Property(objParam, pi);
            var getterExp = Expression.Lambda<Func<T, Object>>(Expression.Convert(propExp, typeof(Object)), objParam);
            return getterExp.Compile();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldType"></param>
        /// <returns>Returned setter action does not handle DBNull. Caller must check DBNull before calling this setter action.</returns>
        private Action<T, IDataRecord, int> CreatePropertyReaderSetter(Type fieldType)
        {
            var pi = this.ObjectProperty;

            var map = new Dictionary<Type, string>()
            {
                { typeof(bool),    "GetBoolean" },
                { typeof(byte),     "GetByte" },
                { typeof(char),     "GetChar" },
                { typeof(DateTime), "GetDateTime" },
                { typeof(Decimal),  "GetDecimal" },
                { typeof(double),   "GetDouble" },
                { typeof(float),    "GetFloat" },
                { typeof(Guid),     "GetGuid" },
                { typeof(Int16),    "GetInt16" },
                { typeof(Int32),    "GetInt32" },
                { typeof(Int64),    "GetInt64" },
                { typeof(string),   "GetString" },
            };

            if (!map.TryGetValue(fieldType, out string funcName))
            {
                throw new NotSupportedException($"The FieldType[{fieldType.FullName}] from IDataReader is not supported. Property[{typeof(T).FullName}.{pi.Name}]");
            }

            var pObj = Expression.Parameter(typeof(T), "obj");
            var pReader = Expression.Parameter(typeof(IDataRecord), "reader");
            var pOrdinary = Expression.Parameter(typeof(int), "ordinary");
            var funGetVal = Expression.Call(pReader, funcName, null, pOrdinary);
            Expression assignProp;
            if (fieldType != pi.PropertyType)
            {
                assignProp = AssignExp(pi, pObj, Expression.Convert(funGetVal, pi.PropertyType));
            }
            else
            {
                assignProp = AssignExp(pi, pObj, funGetVal);
            }
            return Expression.Lambda<Action<T, IDataRecord, int>>(assignProp, pObj, pReader, pOrdinary).Compile();
        }
    }
}
