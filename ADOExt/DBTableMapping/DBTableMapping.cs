using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public class DBTableMapping<T>
    {
        public IReadOnlyList<IDBTableColumnMapping<T>> ColumnMappingList { get; }

        public string TableName { get; private set; }
        public string Schema { get; private set; }

        public DBTableMapping(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans)
        {
            var table = DBTableAttribute.GetTableName(typeof(T));
            if (table == null)
            {
                throw new InvalidOperationException("DBTableAttribute is missing on class " + typeof(T).FullName);
            }
            TableName = table.TableName;
            Schema = table.Schema;

            var resolverProvider = currentConnection.DBService;
            var mapping = resolverProvider.DBObjectMappingFactory.Get<T>();

            Sql sql = resolverProvider.SqlResolver.ColumnMetaDataFromTable(TableName, Schema);
            var metadatas = currentConnection.Query<DBTableMetadata>(sql, currentTrans).ToDictionary(i => i.COLUMN_NAME.ToUpper());
            ColumnMappingList = mapping.ColumnMappingList
                .Select(i =>
                {
                    if (metadatas.TryGetValue(i.ColumnName.ToUpper(), out DBTableMetadata meta)) {
                        return (IDBTableColumnMapping<T>)new DBTableColumnMapping<T>(i, meta);
                    }
                    throw new Exception($"Failed to map property:{i.ColumnName} to the table:{table.TableName}.");
                }).ToList();
        }
    }
}
