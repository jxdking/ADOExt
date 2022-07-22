using System;
using System.Collections.Concurrent;

namespace MagicEastern.ADOExt
{
    public class DBObjectMappingFactory : IDBObjectMappingFactory
    {
        private readonly ConcurrentDictionary<Type, object> _Cache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Return a singleton instance.</returns>
        public DBObjectMapping<T> Get<T>()
        {
            var t = typeof(T);
            return (DBObjectMapping<T>)_Cache.GetOrAdd(t, (_) => new DBObjectMapping<T>());
        }
    }
}
