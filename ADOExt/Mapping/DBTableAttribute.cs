using System;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public class DBTableAttribute : Attribute, IDBTableAttribute
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

        public static IDBTableAttribute GetTableName(Type type)
        {
            //MetadataTypeAttribute metadata = type.GetCustomAttributes(typeof(MetadataTypeAttribute), false).FirstOrDefault() as MetadataTypeAttribute;
            //if (metadata != null)
            //{
            //    type = metadata.MetadataClassType;
            //}
            IDBTableAttribute table = type.GetCustomAttributes(typeof(DBTableAttribute), false).FirstOrDefault() as DBTableAttribute;
            return table;
        }
    }
}