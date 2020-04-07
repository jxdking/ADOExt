using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapterContext<T>
    {
        public readonly IEnumerable<string> InsertColumns;
        public readonly IEnumerable<IDBColumnMapping<T>> InsertColumnsInfo;
        public readonly IEnumerable<string> AllColumns;
        public readonly IEnumerable<IDBColumnMapping<T>> AllColumnsInfo;
        public readonly IEnumerable<string> PkColumns;
        public readonly IEnumerable<IDBColumnMapping<T>> PkColumnsInfo;
        public readonly IEnumerable<string> SetColumns;
        public readonly IEnumerable<IDBColumnMapping<T>> SetColumnsInfo;

        public DBTableMapping<T> Mapping = null;
        public IResolverProvider ResolverProvider = null;

        public DBTableAdapterContext(
            DBTableMapping<T> mapping,
            IResolverProvider resolverProvider,
            IEnumerable<IDBColumnMapping<T>> allCols,
            IEnumerable<IDBColumnMapping<T>> pkCols,
            IEnumerable<IDBColumnMapping<T>> insertableCols,
            IEnumerable<IDBColumnMapping<T>> updatableCols)
        {
            Mapping = mapping;
            ResolverProvider = resolverProvider;
            AllColumnsInfo = allCols;
            PkColumnsInfo = pkCols;
            InsertColumnsInfo = insertableCols;
            SetColumnsInfo = updatableCols;
            AllColumns = allCols.Select(i => i.ColumnName);
            PkColumns = pkCols.Select(i => i.ColumnName);
            InsertColumns = insertableCols.Select(i => i.ColumnName);
            SetColumns = updatableCols.Select(i => i.ColumnName);
        }

        public DBTableAdapterContext(IResolverProvider resolverProvider, DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) 
            : this(DBTableMapping<T>.Get(resolverProvider, currentConnection, currentTrans))
        {
            ResolverProvider = resolverProvider;
        }

        private DBTableAdapterContext(DBTableMapping<T> mapping)
        {
            Mapping = mapping;

            var allColumnsInfo = new List<IDBColumnMapping<T>>();
            var pkColumnsInfo = new List<IDBColumnMapping<T>>();
            var insertColumnsInfo = new List<IDBColumnMapping<T>>();
            var setColumnsInfo = new List<IDBColumnMapping<T>>();
            var allColumns = new List<string>();
            var pkColumns = new List<string>();
            var insertColumns = new List<string>();
            var setColumns = new List<string>();

            foreach (var c in mapping.ColumnMappingList)
            {
                if (!c.NoInsert) { insertColumns.Add(c.ColumnName); insertColumnsInfo.Add(c); }
                allColumns.Add(c.ColumnName);
                allColumnsInfo.Add(c);
                if (c.PK) { pkColumns.Add(c.ColumnName); pkColumnsInfo.Add(c); }
                else if (!c.NoUpdate) { setColumns.Add(c.ColumnName); setColumnsInfo.Add(c); }
            }

            AllColumnsInfo = allColumnsInfo;
            PkColumnsInfo = pkColumnsInfo;
            InsertColumnsInfo = insertColumnsInfo;
            SetColumnsInfo = setColumnsInfo;
            AllColumns = allColumns;
            PkColumns = pkColumns;
            InsertColumns = insertColumns;
            SetColumns = setColumns;
        }
    }
}
