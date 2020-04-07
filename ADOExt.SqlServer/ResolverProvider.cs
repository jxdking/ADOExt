using System;
using System.Collections.Generic;
using System.Data;

namespace MagicEastern.ADOExt.SqlServer
{
    public class ResolverProvider : ResolverProviderBase
    {
        public override IDBClassResolver DBClassResolver { get; }

        public override ISqlResolver SqlResolver { get; }

        public ResolverProvider(Func<IDbConnection> createConnection) : base(createConnection)
        {
            DBClassResolver = new DBClassResolver(createConnection);
            SqlResolver = new SqlResolver();
        }

        public override IDBTableAdapter<T> GetDBTableAdapter<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans)
        {
            var cb = new CommandBuilder<T>(SqlResolver);
            return DBTableAdapter<T>.Get(this, cb, currentConnection, currentTrans);
        }
    }
}