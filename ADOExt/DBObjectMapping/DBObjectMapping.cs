using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBObjectMapping<T> : IDBObjectMapping<T>
    {
        public List<IDBColumnMapping<T>> ColumnMappingList { get; private set; }

        private readonly ConcurrentDictionary<string, Action<T, IDataRecord>[]> DataReaderSetterCache = new ConcurrentDictionary<string, Action<T, IDataRecord>[]>();

        public Action<T, IDataRecord>[] GetDataReaderSetters(IDataRecord reader)
        {
            Action<T, IDataRecord>[] setters = new Action<T, IDataRecord>[ColumnMappingList.Count];

            int i = 0;
            try
            {
                for (i = 0; i < ColumnMappingList.Count; i++)
                {
                    var colMapping = ColumnMappingList[i];
                    var cName = colMapping.ColumnName;
                    var ordinal = reader.GetOrdinal(cName);
                    var setter = colMapping.GetPropSetterForRecord(reader.GetFieldType(ordinal));
                    if (colMapping.Required)
                    {
                        setters[i] = (o, r) => { setter(o, r, ordinal); };
                    }
                    else
                    {
                        setters[i] = (o, r) =>
                        {
                            if (!r.IsDBNull(ordinal))
                            {
                                setter(o, r, ordinal);
                            }
                        };
                    }
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new IndexOutOfRangeException("Unable to find specified column[" + ColumnMappingList[i].ColumnName + "] in DataReader", ex);
            }

            return setters;
        }

        public Action<T, IDataRecord>[] GetDataReaderSetters(string sqltxt, IDataRecord reader)
        {
            return DataReaderSetterCache.GetOrAdd(sqltxt, (_) =>
            {
                return GetDataReaderSetters(reader);
            });
        }

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
