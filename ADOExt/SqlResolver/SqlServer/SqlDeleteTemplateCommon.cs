using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

namespace MagicEastern.ADOExt.Common.SqlServer
{
    public class SqlDeleteTemplateCommon<T, TParameter> : SqlDeleteTemplateBase<T, TParameter>
        where TParameter : IDbDataParameter, new()
    {
        private DBTableAdapterContext<T> tableContext;
        private string tableName;
        private ISqlResolver sqlResolver;

        public SqlDeleteTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
        {
            tableContext = context;
            tableName = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            this.sqlResolver = sqlResolver;
        }

        private string GetTemplateString(string parameterSuffix = null)
        {
            
            var template = "delete from " + tableName + " where " + string.Join(" and ", tableContext.PkColumnsInfo.Select(i => "[" + i.ColumnName + "]=@" + i.ColumnName + parameterSuffix));
            return template;
        }

        public override Sql Generate(T obj, string parameterSuffix = null)
        {
            var sql = new Sql(GetTemplateString(parameterSuffix), tableContext.PkColumnsInfo.Select(i =>
                (IDbDataParameter)sqlResolver.CreateParameter<T, TParameter>(i, obj, ParameterDirection.Input, parameterSuffix)));
            return sql;
        }

    }
}
