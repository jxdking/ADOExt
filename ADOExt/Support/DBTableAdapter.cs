using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapter<T> : DBTableAdapterBase<T> where T : new()
    {
        private static IDBTableAdapter<T>[] _Cache = new DBTableAdapterBase<T>[Constant.MaxResolverProvidorIdx];
        public static IDBTableAdapter<T> Get(IResolverProvider resolverProvider, CommandBuilderFactoryBase commandBuilderFactory, DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans)
        {
            var ret = _Cache[resolverProvider.Idx];
            if (ret == null)
            {
                var commandBuilder = commandBuilderFactory.CreateCommandBuilder<T>(resolverProvider.SqlResolver);
                return _Cache[resolverProvider.Idx] = new DBTableAdapter<T>(resolverProvider, commandBuilder, currentConnection, currentTrans);
            }
            return ret;
        }

        private DBTableAdapter(IResolverProvider resolverProvider, ICommandBuilder<T> commandBuilder, DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans)
            : base(resolverProvider, commandBuilder, currentConnection, currentTrans)
        {
        }
    }
}
