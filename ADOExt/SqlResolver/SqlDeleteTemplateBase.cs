using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MagicEastern.ADOExt
{
    public abstract class SqlDeleteTemplateBase<T, TParameter> : ISqlDeleteTemplate<T>
        where TParameter : IDbDataParameter, new()
    {
        public abstract Sql Generate(T obj, string parameterSuffix = null);
    }
}
