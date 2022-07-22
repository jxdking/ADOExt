using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class DBTableMappingFactory : IDBTableMappingFactory
    {
        private readonly ConcurrentDictionary<Type, object> _Cache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Return a singleton instance.</returns>
        public DBTableMapping<T> Get<T>(DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans)
        {
            var t = typeof(T);
            return (DBTableMapping<T>)_Cache.GetOrAdd(t, (_) => new DBTableMapping<T>(currentConnection, currentTrans));
        }
    }
}
