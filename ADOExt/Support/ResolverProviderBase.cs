using System;
using System.Collections.Specialized;
using System.Threading;

namespace MagicEastern.ADOExt
{
    public abstract class ResolverProviderBase : IResolverProvider
    {
        public abstract IDBClassResolver DBClassResolver { get; }
        public abstract ISqlResolver SqlResolver { get; }
        public string ConnectionString { get; }

        public string ConnectionStringOpened { get; private set; }

        private static int _NextIdx = 0;
        public int Idx { get; }

        public ResolverProviderBase(string connStr)
        {
            Idx = Interlocked.Increment(ref _NextIdx) - 1;
            ConnectionString = connStr;
        }

        private static object _LockRP = new object();
        private static ListDictionary _CacheRP = new ListDictionary();

        public static void Register(ResolverProviderBase provider)
        {
            using (var conn = provider.DBClassResolver.CreateConnection())
            {
                conn.Open();
                provider.ConnectionStringOpened = conn.ConnectionString;
            }

            lock (_LockRP)
            {
                try
                {
                    _CacheRP.Add(provider.ConnectionString, provider);
                    if (provider.ConnectionString != provider.ConnectionStringOpened)
                    {
                        _CacheRP.Add(provider.ConnectionStringOpened, provider);
                    }
                }
                catch (ArgumentException ex)
                {
                    throw new ArgumentException("Cannot register same ConnectionString multiple times.", ex);
                }
            }
        }

        internal static IResolverProvider Get(string connStr)
        {
            var rp = _CacheRP[connStr];
            if (rp == null)
            {
                throw new InvalidOperationException("Failed to get ResolverProvider for Connection String [" + connStr + "]. Did you forget to call ResolverProvider.Register before using Table Mapping function?");
            }
            return (IResolverProvider)rp;
        }

        public DBTableAdapter<T> GetDBTableAdapter<T>() where T : new()
        {
            return DBTableAdapter<T>.Get(this);
        }
    }
}