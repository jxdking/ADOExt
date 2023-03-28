using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public abstract class SqlLoadTemplateBase<T, TParameter> : ISqlLoadTemplate<T>
        where TParameter : IDbDataParameter, new()
    {
        private string Template;
        private readonly ISqlResolver sqlResolver;
        private IEnumerable<IDBTableColumnMapping<T>> PkCols;

        public SqlLoadTemplateBase(IEnumerable<IDBTableColumnMapping<T>> pkCols, string template, ISqlResolver sqlResolver)
        {
            PkCols = pkCols;
            Template = template;
            this.sqlResolver = sqlResolver;
        }

        public Sql Generate(T obj)
        {
            var sql = new Sql(Template, PkCols.Select(i =>
                (IDbDataParameter)sqlResolver.CreateParameter<T, TParameter>(i, obj, ParameterDirection.Input)));
            sql.SchemaCachePriority = CacheItemPriority.NeverRemove;
            return sql;
        }
    }
}
