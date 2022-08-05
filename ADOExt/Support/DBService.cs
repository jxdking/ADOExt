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
        public IDBCommandBuilder DBCommandBuilder { get; }
        public ISqlResolver SqlResolver { get; }

        public DBService(IServiceProvider sp, IDBCommandBuilder dbClassResolver, ISqlResolver sqlResolver, ConnectionFactory connectionFactory)
        {
            DBServiceProvider = sp;
            DBCommandBuilder = dbClassResolver;
            SqlResolver = sqlResolver;
            this.connectionFactory = connectionFactory;
        }
    }
}