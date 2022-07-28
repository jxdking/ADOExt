using System;
using System.Collections.Concurrent;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapterFactory : IDBTableAdapterFactory
    {
        private readonly ConcurrentDictionary<Type, object> _Cache = new ConcurrentDictionary<Type, object>();

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
                return new DBTableAdapter<T>(currentConnection, currentTrans);
            });
        }
    }
}
