using MagicEastern.CachedFunc2;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

//using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    internal class ReaderSettersFactoryPara : IEquatable<ReaderSettersFactoryPara>
    {
        public Sql Sql;
        public IDataRecord Reader;

        public bool Equals(ReaderSettersFactoryPara other)
        {
            return this.Sql.Text.Equals(other?.Sql.Text);
        }
        public override bool Equals(object obj)
        {
            if (obj is ReaderSettersFactoryPara para)
            {
                return Equals(para);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return this.Sql.Text.GetHashCode();
        }
    }

    public class DBObjectMapping<T> : IDBObjectMapping<T>
    {
        public List<IDBColumnMapping<T>> ColumnMappingList { get; private set; }

        public Action<T, IDataRecord>[] GetDataReaderSetters(Sql sql, IDataRecord reader)
            => GetDataReaderSettersCached(new ReaderSettersFactoryPara { Sql = sql, Reader = reader });

        private CachedFunc<ReaderSettersFactoryPara, Action<T, IDataRecord>[]> GetDataReaderSettersCached;

        private Action<T, IDataRecord>[] _GetDataReaderSetters(ReaderSettersFactoryPara para)
        {
            var reader = para.Reader;

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

        public DBObjectMapping(CachedFuncSvc cachedFuncSvc)
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

            GetDataReaderSettersCached = cachedFuncSvc.Create((Func<ReaderSettersFactoryPara, Action<T, IDataRecord>[]>)_GetDataReaderSetters
                , (para) => new MemoryCacheEntryOptions { Priority = para.Sql.SchemaCachePriority, Size = 1 });
        }
    }
}
