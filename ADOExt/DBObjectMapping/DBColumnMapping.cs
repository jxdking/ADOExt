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
        public Func<T, object> PropertyGetter { get; protected set; }
        public Action<T, object> PropertySetter { get; protected set; }

        private readonly ConcurrentDictionary<Type, Action<T, IDataRecord, int>> _CachePropReaderSetters =
            new ConcurrentDictionary<Type, Action<T, IDataRecord, int>>();

        public Action<T, IDataRecord, int> GetPropSetterForRecord(Type fieldType)
        {
            return _CachePropReaderSetters.GetOrAdd(fieldType, _ => CreatePropertyReaderSetter(fieldType));
        }
        #endregion

        public DBColumnMapping()
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
            PropertySetter = CreatePropertyObjectSetter(objectProperty);
            PropertyGetter = CreatePropertyObjectGetter(objectProperty);
        }

        private BinaryExpression AssignExp(PropertyInfo pi, Expression objParam, Expression valParam)
        {
            var propExp = Expression.Property(objParam, pi);
            var assExp = Expression.Assign(propExp, valParam);
            return assExp;
        }

        private Action<T, TProp> CreatePropSetter<TProp>(PropertyInfo pi)
        {
            var pObj = Expression.Parameter(typeof(T), "obj");
            var pVal = Expression.Parameter(typeof(TProp), "val");
            var assign = AssignExp(pi, pObj, pVal);
            return Expression.Lambda<Action<T, TProp>>(assign, pObj, pVal).Compile();
        }

        protected Action<T, object> CreatePropertyObjectSetter(PropertyInfo pi)
        {
            var map = new Dictionary<Type, Func<Action<T, object>>>()
            {
                { typeof(bool),     () => { var setter = CreatePropSetter<bool>(pi);        return (obj, val) => { setter(obj, (bool)val); }; } },
                { typeof(byte),     () => { var setter = CreatePropSetter<byte>(pi);        return (obj, val) => { setter(obj, (byte)val); }; } },
                { typeof(char),     () => { var setter = CreatePropSetter<char>(pi);        return (obj, val) => { setter(obj, (char)val); }; } },
                { typeof(DateTime), () => { var setter = CreatePropSetter<DateTime>(pi);    return (obj, val) => { setter(obj, (DateTime)val); }; } },
                { typeof(Decimal),  () => { var setter = CreatePropSetter<Decimal>(pi);     return (obj, val) => { setter(obj, (Decimal)val); }; } },
                { typeof(double),   () => { var setter = CreatePropSetter<double>(pi);      return (obj, val) => { setter(obj, (double)val); }; } },
                { typeof(float),    () => { var setter = CreatePropSetter<float>(pi);       return (obj, val) => { setter(obj, (float)val); }; } },
                { typeof(Guid),     () => { var setter = CreatePropSetter<Guid>(pi);        return (obj, val) => { setter(obj, (Guid)val); }; } },
                { typeof(Int16),    () => { var setter = CreatePropSetter<Int16>(pi);       return (obj, val) => { setter(obj, (Int16)val); }; } },
                { typeof(Int32),    () => { var setter = CreatePropSetter<Int32>(pi);       return (obj, val) => { setter(obj, (Int32)val); }; } },
                { typeof(Int64),    () => { var setter = CreatePropSetter<Int64>(pi);       return (obj, val) => { setter(obj, (Int64)val); }; } },

                { typeof(bool?),     () => { var setter = CreatePropSetter<bool?>(pi);        return (obj, val) => { setter(obj, (bool?)val); }; } },
                { typeof(byte?),     () => { var setter = CreatePropSetter<byte?>(pi);        return (obj, val) => { setter(obj, (byte?)val); }; } },
                { typeof(char?),     () => { var setter = CreatePropSetter<char?>(pi);        return (obj, val) => { setter(obj, (char?)val); }; } },
                { typeof(DateTime?), () => { var setter = CreatePropSetter<DateTime?>(pi);    return (obj, val) => { setter(obj, (DateTime?)val); }; } },
                { typeof(Decimal?),  () => { var setter = CreatePropSetter<Decimal?>(pi);     return (obj, val) => { setter(obj, (Decimal?)val); }; } },
                { typeof(double?),   () => { var setter = CreatePropSetter<double?>(pi);      return (obj, val) => { setter(obj, (double?)val); }; } },
                { typeof(float?),    () => { var setter = CreatePropSetter<float?>(pi);       return (obj, val) => { setter(obj, (float?)val); }; } },
                { typeof(Guid?),     () => { var setter = CreatePropSetter<Guid?>(pi);        return (obj, val) => { setter(obj, (Guid?)val); }; } },
                { typeof(Int16?),    () => { var setter = CreatePropSetter<Int16?>(pi);       return (obj, val) => { setter(obj, (Int16?)val); }; } },
                { typeof(Int32?),    () => { var setter = CreatePropSetter<Int32?>(pi);       return (obj, val) => { setter(obj, (Int32?)val); }; } },
                { typeof(Int64?),    () => { var setter = CreatePropSetter<Int64?>(pi);       return (obj, val) => { setter(obj, (Int64?)val); }; } },

                { typeof(string),   () => { var setter = CreatePropSetter<string>(pi);      return (obj, val) => { setter(obj, (string)val); }; } },
            };

            if (!map.TryGetValue(pi.PropertyType, out Func<Action<T, object>> factory))
            {
                throw new NotSupportedException($"The type[{pi.PropertyType.FullName}] of the Property[{typeof(T).FullName}.{pi.Name}] is not supported.");
            }
            var propSetter = factory();
            Action<T, object> nullableSetter = (obj, val) =>
        {
            if (DBNull.Value.Equals(val))
            {
                propSetter(obj, null);
            }
            else
            {
                propSetter(obj, val);
            }
        };

            if (Required)
            {
                return propSetter;
            }

            if (pi.PropertyType == typeof(string))
            {
                return nullableSetter;
            }

            var vt = Nullable.GetUnderlyingType(pi.PropertyType);
            if (vt == null)
            {
                // must be a value type
                return propSetter;
            }

            // must be Nullable<T>
            return nullableSetter;
        }

        private Func<T, Object> CreatePropertyObjectGetter(PropertyInfo pi)
        {
            var objParam = Expression.Parameter(typeof(T), "obj");
            var propExp = Expression.Property(objParam, pi);
            var getterExp = Expression.Lambda<Func<T, Object>>(Expression.Convert(propExp, typeof(Object)), objParam);
            var getterFn = getterExp.Compile();
            return getterFn;
        }

        #region CreatePropertyReaderSetter
        private Action<T, IDataRecord, int> CreatePropSetter<TCol>(Func<IDataRecord, int, TCol> readerGetFunc
            , Action<T, TCol> propSetter) where TCol : struct
        {
            return (obj, reader, ordinal) =>
            {
                propSetter(obj, readerGetFunc(reader, ordinal));
            };
        }

        private Action<T, IDataRecord, int> CreatePropSetter<TCol>(Func<IDataRecord, int, TCol> readerGetFunc
            , Action<T, TCol?> propSetter) where TCol : struct
        {
            if (Required)
            {
                return (obj, reader, ordinal) =>
                {
                    propSetter(obj, readerGetFunc(reader, ordinal));
                };
            }
            return (obj, reader, ordinal) =>
            {
                propSetter(obj, reader.IsDBNull(ordinal) ? default(TCol?) : readerGetFunc(reader, ordinal));
            };
        }

        private Action<T, IDataRecord, int> CreatePropSetter<TCol, TProp>(Func<IDataRecord, int, TCol> readerGetFunc
            , Action<T, TProp> propSetter) where TProp : struct
        {
            return (obj, reader, ordinal) =>
            {
                propSetter(obj, (TProp)Convert.ChangeType(readerGetFunc(reader, ordinal), typeof(TProp)));
            };
        }

        private Action<T, IDataRecord, int> CreatePropSetter<TCol, TProp>(Func<IDataRecord, int, TCol> readerGetFunc
            , Action<T, TProp?> propSetter) where TProp : struct
        {
            if (Required)
            {
                return (obj, reader, ordinal) =>
                {
                    propSetter(obj, (TProp?)Convert.ChangeType(readerGetFunc(reader, ordinal), typeof(TProp)));
                };
            }
            return (obj, reader, ordinal) =>
            {
                propSetter(obj, reader.IsDBNull(ordinal)
                    ? default(TProp?)
                    : (TProp?)Convert.ChangeType(readerGetFunc(reader, ordinal), typeof(TProp)));
            };
        }

        private Action<T, IDataRecord, int> CreatePropSetter(PropertyInfo pi, Func<IDataRecord, int, string> readerGetFunc)
        {
            if (pi.PropertyType == typeof(string))
            {
                var propSetter = CreatePropSetter<string>(pi);
                if (Required)
                {
                    return (obj, reader, ordinal) =>
                    {
                        propSetter(obj, readerGetFunc(reader, ordinal));
                    };
                }
                return (obj, reader, ordinal) =>
                {
                    propSetter(obj, reader.IsDBNull(ordinal) ? null : readerGetFunc(reader, ordinal));
                };
            }
            throw new NotSupportedException($"The property[{typeof(T).FullName}.{pi.Name}] has to be string type to parse the DataReader from the query, but it is in [{pi.PropertyType.FullName}] type.");
        }

        private Action<T, IDataRecord, int> CreatePropSetter<TCol>(PropertyInfo pi, Func<IDataRecord, int, TCol> readerGetFunc)
            where TCol : struct
        {
            var pt = pi.PropertyType;
            var upt = Nullable.GetUnderlyingType(pt);
            var map = new Dictionary<Type, Func<Action<T, IDataRecord, int>>>() {
                { typeof(bool),     () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<bool>(pi)) },
                { typeof(byte),     () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<byte>(pi)) },
                { typeof(char),     () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<char>(pi)) },
                { typeof(DateTime), () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<DateTime>(pi)) },
                { typeof(Decimal),  () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Decimal>(pi)) },
                { typeof(double),   () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<double>(pi)) },
                { typeof(float),    () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<float>(pi)) },
                { typeof(Guid),     () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Guid>(pi)) },
                { typeof(Int16),    () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Int16>(pi)) },
                { typeof(Int32),    () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Int32>(pi)) },
                { typeof(Int64),    () => pt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Int64>(pi)) },

                { typeof(bool?),     () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<bool?>(pi)) },
                { typeof(byte?),     () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<byte?>(pi)) },
                { typeof(char?),     () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<char?>(pi)) },
                { typeof(DateTime?), () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<DateTime?>(pi)) },
                { typeof(Decimal?),  () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Decimal?>(pi)) },
                { typeof(double?),   () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<double?>(pi)) },
                { typeof(float?),    () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<float?>(pi)) },
                { typeof(Guid?),     () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Guid?>(pi)) },
                { typeof(Int16?),    () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Int16?>(pi)) },
                { typeof(Int32?),    () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Int32?>(pi)) },
                { typeof(Int64?),    () => upt == typeof(TCol) ? CreatePropSetter<TCol>(readerGetFunc, CreatePropSetter<TCol?>(pi)) : CreatePropSetter(readerGetFunc, CreatePropSetter<Int64?>(pi)) },
            };

            if (!map.TryGetValue(pt, out var factory))
            {
                throw new NotSupportedException($"The PropertyType[{pt.FullName}] of [{typeof(T).FullName}.{pi.Name}] is not supported.");
            }

            return factory();
        }

        private Action<T, IDataRecord, int> CreatePropertyReaderSetter(Type fieldType)
        {
            var pi = this.ObjectProperty;

            var map = new Dictionary<Type, Func<Action<T, IDataRecord, int>>>()
            {
                { typeof(bool),     () => CreatePropSetter(pi, (reader, ordinary) => reader.GetBoolean(ordinary)) },
                { typeof(byte),     () => CreatePropSetter(pi, (reader, ordinary) => reader.GetByte(ordinary)) },
                { typeof(char),     () => CreatePropSetter(pi, (reader, ordinary) => reader.GetChar(ordinary)) },
                { typeof(DateTime), () => CreatePropSetter(pi, (reader, ordinary) => reader.GetDateTime(ordinary)) },
                { typeof(Decimal),  () => CreatePropSetter(pi, (reader, ordinary) => reader.GetDecimal(ordinary)) },
                { typeof(double),   () => CreatePropSetter(pi, (reader, ordinary) => reader.GetDouble(ordinary)) },
                { typeof(float),    () => CreatePropSetter(pi, (reader, ordinary) => reader.GetFloat(ordinary)) },
                { typeof(Guid),     () => CreatePropSetter(pi, (reader, ordinary) => reader.GetGuid(ordinary)) },
                { typeof(Int16),    () => CreatePropSetter(pi, (reader, ordinary) => reader.GetInt16(ordinary)) },
                { typeof(Int32),    () => CreatePropSetter(pi, (reader, ordinary) => reader.GetInt32(ordinary)) },
                { typeof(Int64),    () => CreatePropSetter(pi, (reader, ordinary) => reader.GetInt64(ordinary)) },

                { typeof(string),   () => CreatePropSetter(pi, (reader, ordinary) => reader.GetString(ordinary)) },
            };

            if (!map.TryGetValue(fieldType, out Func<Action<T, IDataRecord, int>> factory))
            {
                throw new NotSupportedException($"The FieldType[{fieldType.FullName}] from IDataReader is not supported. Property[{typeof(T).FullName}.{pi.Name}]");
            }
            return factory();
        }
        #endregion
    }
}
