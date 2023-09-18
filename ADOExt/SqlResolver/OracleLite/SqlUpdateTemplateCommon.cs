using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.OracleLite
{
    public class SqlUpdateTemplateCommon<T, TParameter> : SqlUpdateTemplateBase<T>
        where T : new()
        where TParameter : IDbDataParameter, new()
    {
        private readonly DBTableAdapterContext<T> context;
        private readonly ISqlResolver sqlResolver;
        private string tableName;

        public SqlUpdateTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
        {
            tableName = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            this.context = context;
            this.sqlResolver = sqlResolver;
        }


        public override Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, string parameterSuffix = null)
        {
            if (!(setCols is List<IDBTableColumnMapping<T>> cols))
            {
                cols = setCols.ToList();
            }
            var sqltxt = $"update {tableName} set {string.Join(",", cols.Select(i => i.ColumnName + "=:" + i.ColumnName + parameterSuffix))} " +
                $"where " + string.Join(" and ", context.PkColumnsInfo.Select(i => i.ColumnName + "=:" + i.ColumnName + parameterSuffix));
            return new Sql(sqltxt, cols.Concat(context.PkColumnsInfo).Select(i => (IDbDataParameter)sqlResolver.CreateParameter<T, TParameter>(i, obj, ParameterDirection.Input, parameterSuffix)));
        }
    }
}
