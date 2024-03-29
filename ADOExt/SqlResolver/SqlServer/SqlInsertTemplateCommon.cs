﻿using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt.Common.SqlServer
{
    public class SqlInsertTemplateCommon<T, TParameter> : SqlInsertTemplateBase<T>
        where TParameter : IDbDataParameter, new()
    {
        private readonly ISqlResolver sqlResolver;
        private string tablename;

        public SqlInsertTemplateCommon(DBTableAdapterContext<T> context, ISqlResolver sqlResolver)
        {
            tablename = sqlResolver.GetTableName(context.Mapping.TableName, context.Mapping.Schema);
            this.sqlResolver = sqlResolver;
        }



        public override Sql Generate(T obj, IEnumerable<IDBTableColumnMapping<T>> setCols, string parameterSuffix = null)
        {
            if (!(setCols is List<IDBTableColumnMapping<T>> cols))
            {
                cols = setCols.ToList();
            }
            
            string sqltxt = $"insert into {tablename} ([{string.Join("],[", cols.Select(i => i.ColumnName))}]) values (@{string.Join(",@", cols.Select(i => i.ColumnName + parameterSuffix))})" ;
            return new Sql(sqltxt, cols.Select(i => (IDbDataParameter)sqlResolver.CreateParameter<T, TParameter>(i, obj, ParameterDirection.Input, parameterSuffix)));
        }
    }
}
