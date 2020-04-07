using System;
using System.Collections.Specialized;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBColumnMapping<T> : IDBColumnMapping<T>
    {
        public string ColumnName { get; protected set; }
        public bool NoInsert { get; protected set; }
        public bool NoUpdate { get; protected set; }
        /// <summary>
        /// for value type column, the Required will always be true.
        /// </summary>
        public bool Required { get; protected set; }

        public PropertyInfo ObjectProperty { get; protected set; }

        public Func<T, object> PropertyGetter { get; protected set; }
        public Action<T, object> PropertySetter { get; protected set; }

        protected object _CacheLockObj = new object();
        protected ListDictionary _CachePropReaderSetters = new ListDictionary();

        public Action<T, IDataRecord, int> GetPropSetterForRecord(Type fieldType)
        {
            object fun;
            lock (_CacheLockObj)
            {
                fun = _CachePropReaderSetters[fieldType];
                if (fun == null)
                {
                    fun = GetPropertyReaderSetter(fieldType);
                    _CachePropReaderSetters.Add(fieldType, fun);
                }
            }

            return (Action<T, IDataRecord, int>)fun;
        }

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
            Required = required || t.IsValueType && Activator.CreateInstance(t) != null;
            ObjectProperty = objectProperty;
            ColumnName = columnName;
            NoInsert = noInsert;
            NoUpdate = noUpdate;
            PropertySetter = CreateSetter(objectProperty, !Required);
            PropertyGetter = CreateGetter(ObjectProperty);
        }

        private Func<T, Object> CreateGetter(PropertyInfo pi)
        {
            var objParam = Expression.Parameter(typeof(T), "obj");
            var propExp = Expression.Property(objParam, pi);
            var getterExp = Expression.Lambda<Func<T, Object>>(Expression.Convert(propExp, typeof(Object)), objParam);
            var getterFn = getterExp.Compile();
            return getterFn;
        }

        private Expression AssignExp(PropertyInfo pi, Expression objParam, Expression valParam)
        {
            var propExp = Expression.Property(objParam, pi);
            var assExp = Expression.Assign(propExp, valParam);
            return assExp;
        }

        private Action<T, IDataRecord, int> CreatePropSetter<TProp>(PropertyInfo pi, Expression<Func<IDataRecord, int, TProp>> readerGetFunc, bool nullable)
        {
            var pt = pi.PropertyType.UnwrapIfNullable();

            var pObj = Expression.Parameter(typeof(T), "obj");
            var pReader = Expression.Parameter(typeof(IDataRecord), "reader");
            var pOrdinary = Expression.Parameter(typeof(int), "ordinary");

            var readerGetExp = Expression.Invoke(readerGetFunc, pReader, pOrdinary);

            var castedVal = (Expression)readerGetExp;

            if (pt != pi.PropertyType && pt == typeof(TProp))
            {
                castedVal = Expression.Convert(readerGetExp, pi.PropertyType);
            }
            if (pt != typeof(TProp))
            {
                var converted = Expression.Call(null, typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) })
                    , Expression.Convert(readerGetExp, typeof(object))
                    , Expression.Constant(pt, typeof(Type)));
                castedVal = Expression.Convert(converted, pi.PropertyType);
            }

            Expression<Func<IDataRecord, int, bool>> isDBNullFunc = (reader, ordinary) => reader.IsDBNull(ordinary);

            var valToAssign = castedVal;
            if (nullable)
            {
                var isDBNull = Expression.Invoke(isDBNullFunc, pReader, pOrdinary);
                valToAssign = Expression.Condition(
                    isDBNull,
                    Expression.Constant(null, pi.PropertyType),
                    castedVal
                );
            }
            var assign = AssignExp(pi, pObj, valToAssign);
            return Expression.Lambda<Action<T, IDataRecord, int>>(assign, pObj, pReader, pOrdinary).Compile();
        }

        protected Action<T, object> CreateSetter(PropertyInfo pi, bool dbNullable = true)
        {
            var pt = pi.PropertyType.UnwrapIfNullable();

            var pObj = Expression.Parameter(typeof(T), "obj");
            var pVal = Expression.Parameter(typeof(object), "val");
            var convertExp = Expression.Call(null, typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) })
                , pVal
                , Expression.Constant(pt, typeof(Type)));
            var castedVal = Expression.Convert(convertExp, pi.PropertyType);

            var valToAssign = (Expression)castedVal;
            if (dbNullable)
            {
                valToAssign = Expression.Condition(
                    Expression.Or(
                        Expression.Equal(Expression.Constant(null), pVal),
                        Expression.Equal(Expression.Constant(DBNull.Value), pVal)
                    ),
                    Expression.Constant(null, pi.PropertyType),
                    castedVal
                );
            }

            var assign = AssignExp(pi, pObj, valToAssign);
            return Expression.Lambda<Action<T, object>>(assign, pObj, pVal).Compile();
        }

        private Action<T, IDataRecord, int> GetPropertyReaderSetter(Type fieldType)
        {
            Action<T, IDataRecord, int> setVal;
            var pi = this.ObjectProperty;
            if (fieldType == typeof(bool))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetBoolean(ordinary), !Required);
            }
            else if (fieldType == typeof(byte))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetByte(ordinary), !Required);
            }
            else if (fieldType == typeof(Char))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetChar(ordinary), !Required);
            }
            else if (fieldType == typeof(DateTime))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetDateTime(ordinary), !Required);
            }
            else if (fieldType == typeof(Decimal))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetDecimal(ordinary), !Required);
            }
            else if (fieldType == typeof(Double))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetDouble(ordinary), !Required);
            }
            else if (fieldType == typeof(float))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetFloat(ordinary), !Required);
            }
            else if (fieldType == typeof(Guid))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetGuid(ordinary), !Required);
            }
            else if (fieldType == typeof(Int16))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetInt16(ordinary), !Required);
            }
            else if (fieldType == typeof(Int32))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetInt32(ordinary), !Required);
            }
            else if (fieldType == typeof(Int64))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetInt64(ordinary), !Required);
            }
            else if (fieldType == typeof(string))
            {
                setVal = CreatePropSetter(pi, (reader, ordinary) => reader.GetString(ordinary), !Required);
            }
            else
            {
                throw new NotSupportedException("Cannot parse " + pi.Name + "(" + fieldType.FullName + ") from IDataRecord");
            }
            return setVal;
        }
    }
}