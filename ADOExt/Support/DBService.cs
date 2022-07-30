using Microsoft.Extensions.DependencyInjection;
using System;

namespace MagicEastern.ADOExt
{
    public class DBService : IDBService
    {
        private readonly ConnectionFactory connectionFactory;

        public DBConnectionWrapper OpenConnection() => connectionFactory();

        public IDBObjectMapping<T> GetDBObjectMapping<T>()
        {
            return DBServiceProvider.GetService<IDBObjectMapping<T>>();
        }

        public IDBTableAdapter<T> GetDBTableAdapter<T>() where T : new()
        {
            return DBServiceProvider.GetService<IDBTableAdapter<T>>();
        }

        public IServiceProvider DBServiceProvider { get; }
        public IDBClassResolver DBClassResolver { get; }
        public ISqlResolver SqlResolver { get; }

        public DBService(IServiceProvider sp, IDBClassResolver dbClassResolver, ISqlResolver sqlResolver, ConnectionFactory connectionFactory)
        {
            DBServiceProvider = sp;
            DBClassResolver = dbClassResolver;
            SqlResolver = sqlResolver;
            this.connectionFactory = connectionFactory;
        }
    }
}