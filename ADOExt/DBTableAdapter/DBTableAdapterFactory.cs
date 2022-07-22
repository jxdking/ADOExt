using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapterFactory : IDBTableAdapterFactory
    {
        private readonly ConcurrentDictionary<Type, object> _Cache = new ConcurrentDictionary<Type, object>();

        public ICommandBuilderFactory CommandBuilderFactory { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandBuilder"></param>
        /// <param name="currentConnection"></param>
        /// <param name="currentTrans"></param>
        /// <returns>Get a singleton instance.</returns>
        public DBTableAdapter<T> Get<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) where T : new()
        {
            var t = typeof(T);
            return (DBTableAdapter<T>)_Cache.GetOrAdd(t, (_) =>
            {
                var commandBuilder = CommandBuilderFactory.CreateCommandBuilder<T>(currentConnection.DBService.SqlResolver);
                return new DBTableAdapter<T>(commandBuilder, currentConnection, currentTrans);
            });
        }

        public DBTableAdapterFactory(ICommandBuilderFactory commandBuilderFactory)
        {
            CommandBuilderFactory = commandBuilderFactory;
        }
    }
}
