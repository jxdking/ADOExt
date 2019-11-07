using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public class DBTableMapping<T>
    {
        public IEnumerable<IDBColumnMappingInfo<T>> ColumnMappingList { get; }

        public string TableName => _TableName;
        public string Schema => _Schema;

        private static string _TableName;
        private static string _Schema;

        static DBTableMapping()
        {
            var table = DBTableAttribute.GetTableName(typeof(T));
            if (table == null)
            {
                throw new InvalidOperationException("DBTableAttribute is missing on class " + typeof(T).FullName);
            }
            _TableName = table.TableName;
            _Schema = table.Schema;
        }

        private static DBTableMapping<T>[] _Cache = new DBTableMapping<T>[Constant.MaxResolverProvidorIdx];

        public static DBTableMapping<T> Get(IResolverProvider resolverProvider)
        {
            var ret = _Cache[resolverProvider.Idx];
            if (ret == null)
            {
                return _Cache[resolverProvider.Idx] = new DBTableMapping<T>(resolverProvider);
            }
            return ret;
        }

        private DBTableMapping(IResolverProvider resolverProvider)
        {
            var mapping = DBObjectMapping<T>.Get();

            Sql sql = resolverProvider.SqlResolver.ColumnMetaDataFromTable(TableName, Schema);

            using (var conn = resolverProvider.DBClassResolver.CreateConnection())
            {
                conn.Open();
                List<SchemaMetadata> metadatas = conn.Query<SchemaMetadata>(sql);

                ColumnMappingList = mapping.ColumnMappingList
                    .Select(i => new DBColumnMappingInfo<T>(i, metadatas.Single(j => string.Compare(j.COLUMN_NAME, i.ColumnName, true) == 0)));
            }
        }
    }
}