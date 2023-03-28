using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public abstract class SqlDeleteTemplateBase<T, TParameter> : ISqlDeleteTemplate<T>
        where TParameter : IDbDataParameter, new()
    {
        private string Template;
        private readonly ISqlResolver sqlResolver;
        private IEnumerable<IDBTableColumnMapping<T>> PkCols;

        protected SqlDeleteTemplateBase(IEnumerable<IDBTableColumnMapping<T>> pkCols, string template, ISqlResolver sqlResolver)
        {
            Template = template;
            this.sqlResolver = sqlResolver;
            PkCols = pkCols;
        }

        public Sql Generate(T obj)
        {
            var sql = new Sql(Template, PkCols.Select(i =>
                (IDbDataParameter)sqlResolver.CreateParameter<T, TParameter>(i, obj, ParameterDirection.Input)));
            return sql;
        }
    }
}
