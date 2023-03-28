using MagicEastern.ADOExt.Common.Oracle;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace MagicEastern.ADOExt.Oracle
{
    public class SqlResolver : SqlResolverCommon<OracleParameter>
    {
        public SqlResolver(IServiceProvider sp) : base(sp)
        {
        }

        public override void ConfigureParameter<T>(IDbDataParameter p, IDBTableColumnMapping<T> col, T obj, ParameterDirection direction)
        {
            if (!(p is OracleParameter oraclePara))
            {
                throw new ArgumentException("[p] has to be OracleParameter type");
            }

            var mapping = new Dictionary<string, OracleDbType>() {
                { "CLOB", OracleDbType.Clob},
                { "NCLOB", OracleDbType.NClob }
            };

            base.ConfigureParameter(oraclePara, col, obj, direction);

            if (mapping.TryGetValue(col.DataType, out OracleDbType ot))
            {
                oraclePara.OracleDbType = ot;
            }
        }
    }
}
