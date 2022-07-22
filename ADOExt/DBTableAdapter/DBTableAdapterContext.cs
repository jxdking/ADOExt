using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapterContext<T>
    {
        public readonly IReadOnlyList<string> InsertColumns;
        public readonly IReadOnlyList<IDBColumnMapping<T>> InsertColumnsInfo;
        public readonly IReadOnlyList<string> AllColumns;
        public readonly IReadOnlyList<IDBColumnMapping<T>> AllColumnsInfo;
        public readonly IReadOnlyList<string> PkColumns;
        public readonly IReadOnlyList<IDBColumnMapping<T>> PkColumnsInfo;
        public readonly IReadOnlyList<string> SetColumns;
        public readonly IReadOnlyList<IDBColumnMapping<T>> SetColumnsInfo;

        public DBTableMapping<T> Mapping = null;
        public IDBService DBService = null;

        public DBTableAdapterContext(
            DBTableMapping<T> mapping,
            IDBService dbService,
            List<IDBColumnMapping<T>> allCols,
            List<IDBColumnMapping<T>> pkCols,
            List<IDBColumnMapping<T>> insertableCols,
            List<IDBColumnMapping<T>> updatableCols)
        {
            Mapping = mapping;
            DBService = dbService;
            AllColumnsInfo = allCols;
            PkColumnsInfo = pkCols;
            InsertColumnsInfo = insertableCols;
            SetColumnsInfo = updatableCols;
            AllColumns = allCols.Select(i => i.ColumnName).ToList();
            PkColumns = pkCols.Select(i => i.ColumnName).ToList();
            InsertColumns = insertableCols.Select(i => i.ColumnName).ToList();
            SetColumns = updatableCols.Select(i => i.ColumnName).ToList();
        }

        public DBTableAdapterContext(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) 
            : this(currentConnection.DBService.DBTableMappingFactory.Get<T>(currentConnection, currentTrans))
        {
            DBService = currentConnection.DBService;
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
