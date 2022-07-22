using System;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public class DBTableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string Schema { get; set; }

        public DBTableAttribute()
        {
        }

        public DBTableAttribute(string tableName)
        {
            TableName = tableName;
        }

        public static DBTableAttribute GetTableName(Type type)
        {
            DBTableAttribute table = type.GetCustomAttributes(typeof(DBTableAttribute), false).FirstOrDefault() as DBTableAttribute;
            return table;
        }
    }
}