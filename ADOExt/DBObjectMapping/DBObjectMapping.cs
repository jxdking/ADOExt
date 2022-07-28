using System.Collections.Generic;

//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBObjectMapping<T>
    {
        public IReadOnlyList<IDBColumnMapping<T>> ColumnMappingList { get; private set; }

        public DBObjectMapping()
        {
            var columnMappingList = new List<IDBColumnMapping<T>>();

            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (var p in properties)
            {
                if (p.CanWrite && p.CanRead)
                {
                    if (p.GetCustomAttributes(typeof(DBColumnAttribute), true).FirstOrDefault() is DBColumnAttribute dbColAtt)
                    {
                        var colName = string.IsNullOrEmpty(dbColAtt.ColumnName) ? p.Name : dbColAtt.ColumnName;
                        columnMappingList.Add(
                            new DBColumnMapping<T>(
                                p,
                                colName,
                                dbColAtt.Required,
                                dbColAtt.NoInsert,
                                dbColAtt.NoUpdate
                            )
                        );
                    }
                }
            }

            ColumnMappingList = columnMappingList;
        }
    }
}
