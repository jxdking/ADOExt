﻿using System;
using System.Collections.Generic;
using System.Data;

namespace MagicEastern.ADOExt
{
    public class Parameter : IDataParameter , IEquatable<Parameter>
    {
        private static readonly Dictionary<Type, DbType> DbTypeDic = new Dictionary<Type, DbType>() {
           { typeof(bool?), DbType.Boolean          },
           { typeof(byte?), DbType.Byte             },
           { typeof(DateTime?), DbType.DateTime     },
           { typeof(Decimal?), DbType.Decimal       },
           { typeof(double?), DbType.Double         },
           { typeof(float?), DbType.Double          },
           { typeof(Guid?), DbType.Guid             },
           { typeof(Int16?), DbType.Int16           },
           { typeof(Int32?), DbType.Int32           },
           { typeof(Int64?), DbType.Int64           },
           { typeof(string), DbType.String         },
           { typeof(bool), DbType.Boolean          },
           { typeof(byte), DbType.Byte             },
           { typeof(DateTime), DbType.DateTime     },
           { typeof(Decimal), DbType.Decimal       },
           { typeof(double), DbType.Double         },
           { typeof(float), DbType.Double          },
           { typeof(Guid), DbType.Guid             },
           { typeof(Int16), DbType.Int16           },
           { typeof(Int32), DbType.Int32           },
           { typeof(Int64), DbType.Int64           },
        };

        public string ParameterName { get; set; }
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;

        private DbType? dbType;
        public DbType DbType
        {
            get
            {
                if (dbType != null)
                {
                    return (DbType)dbType;
                }
                var t = ObjectType;
                if (t != null)
                {
                    var ut = Nullable.GetUnderlyingType(t);
                    t = ut ?? t;
                    if (DbTypeDic.TryGetValue(t, out var dbt))
                    {
                        return dbt;
                    }
                }
                throw new NotSupportedException($"DbType is never set, and failed to map from the object Type[{"" + _value?.GetType().FullName}] to DbType.");
            }
            set
            {
                dbType = value;
            }
        }

        /// <summary>
        /// ObjectType can imply the DbType, in case that DbType is never set and the current value is null.
        /// </summary>
        public Type ObjectType { get; set; }
        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                if (value != null)
                {
                    ObjectType = value.GetType();
                }
            }
        }

        public bool IsNullable => throw new NotImplementedException();

        public string SourceColumn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DataRowVersion SourceVersion { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Parameter()
        {

        }

        public Parameter(string name, object value)
        {
            ParameterName = name;
            Value = value;
        }

        public override string ToString()
        {
            return new KeyValuePair<string, object>(ParameterName, Value).ToString();
        }

        #region IEquatable
        public bool Equals(Parameter other)
        {
            if (other == null) { return false; }
            return other.ParameterName == ParameterName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Parameter);
        }

        public override int GetHashCode()
        {
            return ParameterName.GetHashCode();
        }
        #endregion
    }
}