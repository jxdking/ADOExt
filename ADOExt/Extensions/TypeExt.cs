using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace MagicEastern.ADOExt
{
    public static class TypeExt
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
        public static DbType ToDbType(this Type t) {
            if (!DbTypeDic.TryGetValue(t, out DbType res)) {
                throw new NotSupportedException($"Type[{t.FullName}] is not supported.");
            }
            return res;
        }
    }
}
