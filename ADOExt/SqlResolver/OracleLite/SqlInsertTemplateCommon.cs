using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace MagicEastern.ADOExt.Common.OracleLite
{
    public class SqlInsertTemplateCommon<T, TParameter> : SqlInsertTemplateBase<T>
        where TParameter : IDbDataParameter, new()
        where T : new()
    {
        private readonly ISqlResolver sqlResolver;
        private string tableName;

        public SqlInsertTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
        {
            tableName = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            this.sqlResolver = sqlResolver;
        }



        public override Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, string parameterSuffix = null)
        {
            if (!(setCols is List<IDBTableColumnMapping<T>> cols))
            {
                cols = setCols.ToList();
            }

            string sqltxt = $"insert into {tableName} ({string.Join(",", cols.Select(i => i.ColumnName))}) values (:{string.Join(",:", cols.Select(i => i.ColumnName + parameterSuffix))})";
            return new Sql(sqltxt, cols.Select(i => (IDbDataParameter)sqlResolver.CreateParameter<T, TParameter>(i, obj, ParameterDirection.Input, parameterSuffix)));
        }
    }
}
