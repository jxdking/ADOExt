using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapterContext<T>
    {
        public readonly List<IDBTableColumnMapping<T>> InsertColumnsInfo;
        public readonly List<IDBTableColumnMapping<T>> AllColumnsInfo;
        public readonly List<IDBTableColumnMapping<T>> PkColumnsInfo;
        public readonly List<IDBTableColumnMapping<T>> SetColumnsInfo;

        public IDBTableMapping<T> Mapping = null;

        public DBTableAdapterContext(IDBTableMapping<T> mapping)
        {
            Mapping = mapping;

            var allColumnsInfo = new List<IDBTableColumnMapping<T>>();
            var pkColumnsInfo = new List<IDBTableColumnMapping<T>>();
            var insertColumnsInfo = new List<IDBTableColumnMapping<T>>();
            var setColumnsInfo = new List<IDBTableColumnMapping<T>>();
        
            for (int i = 0; i < mapping.ColumnMappingList.Count; i++)
            {
                var c = mapping.ColumnMappingList[i];
                if (!c.NoInsert)
                {
                    insertColumnsInfo.Add(c);
                }
                allColumnsInfo.Add(c);
                if (c.PK)
                {
                    pkColumnsInfo.Add(c);
                }
                else if (!c.NoUpdate)
                {
                    setColumnsInfo.Add(c);
                }
            }

            AllColumnsInfo = allColumnsInfo;
            PkColumnsInfo = pkColumnsInfo;
            InsertColumnsInfo = insertColumnsInfo;
            SetColumnsInfo = setColumnsInfo;
        }
    }
}
