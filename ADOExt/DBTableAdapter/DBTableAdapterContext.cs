using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapterContext<T>
    {
        public readonly List<string> InsertColumns;
        public readonly List<IDBColumnMapping<T>> InsertColumnsInfo;
        public readonly List<string> AllColumns;
        public readonly List<IDBColumnMapping<T>> AllColumnsInfo;
        public readonly List<string> PkColumns;
        public readonly List<IDBColumnMapping<T>> PkColumnsInfo;
        public readonly List<string> SetColumns;
        public readonly List<IDBColumnMapping<T>> SetColumnsInfo;

        public IDBTableMapping<T> Mapping = null;

        public DBTableAdapterContext(IDBTableMapping<T> mapping)
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

            for (int i = 0; i < mapping.ColumnMappingList.Count; i++)
            {
                var c = mapping.ColumnMappingList[i];
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
