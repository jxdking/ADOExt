using System;
using System.Data;

namespace MagicEastern.ADOExt
{
    public class ResolverProvider : ResolverProviderBase
    {
        private CommandBuilderFactoryBase _CommandBuilderFactory;

        public ResolverProvider(IDBClassResolver dbClassResolver, ISqlResolver sqlResolver, CommandBuilderFactoryBase commandBuilderFactory) : base(dbClassResolver, sqlResolver) 
        {
            _CommandBuilderFactory = commandBuilderFactory;
        }

        public override IDBTableAdapter<T> GetDBTableAdapter<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans)
        {
            return DBTableAdapter<T>.Get(this, _CommandBuilderFactory, currentConnection, currentTrans);
        }
    }
}