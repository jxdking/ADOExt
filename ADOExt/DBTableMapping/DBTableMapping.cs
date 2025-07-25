﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public class DBTableMapping<T> : IDBTableMapping<T>
    {
        public List<IDBTableColumnMapping<T>> ColumnMappingList { get; }

        public string TableName { get; }
        public string Schema { get; }

        public DBTableMapping(ConnectionFactory openConnection, ISqlResolver sqlResolver, IDBObjectMapping<T> mapping)
        {
            var table = DBTableAttribute.GetTableName(typeof(T));
            if (table == null)
            {
                throw new InvalidOperationException("DBTableAttribute is missing on class " + typeof(T).FullName);
            }
            TableName = table.TableName;
            Schema = table.Schema;

            Sql sql = sqlResolver.ColumnMetaDataFromTable(TableName, Schema);

            using (var conn = openConnection())
            {
                var qry = conn.Query<DBTableMetadata>(sql);

                Dictionary<string, DBTableMetadata> metadatas;
                try
                {
                    metadatas = qry.ToDictionary(i => i.COLUMN_NAME.ToUpper());
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"Duplicated records found for [table:{TableName}], [schema:{Schema}]. Make sure you have defined the [schema] to avoid ambiguous.", ex);
                }

                ColumnMappingList = mapping.ColumnMappingList
                    .Select(i =>
                    {
                        if (metadatas.TryGetValue(i.ColumnName.ToUpper(), out DBTableMetadata meta))
                        {
                            return (IDBTableColumnMapping<T>)new DBTableColumnMapping<T>(i, meta);
                        }
                        throw new Exception($"Failed to map property:{i.ColumnName} to the table:{table.TableName}.");
                    }).ToList();
            }
            if (ColumnMappingList.Count == 0)
            {
                throw new Exception($"Failed to get metadata information from database, Table[{TableName}], Schema[{Schema}].");
            }
        }
    }
}
