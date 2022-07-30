using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public class DBServiceManager : IDBService
    {
        private readonly Dictionary<string, IDBService> _DBServices = new Dictionary<string, IDBService>();
        private IDBService Default = null;

        private void AddConfig(object sender, ConfigAddedEventArgs args)
        {
            var svc = args.ServiceProvider.GetService<IDBService>();
            _DBServices[args.ConfigName] = svc;
            if (Default == null)
            {
                Default = svc;
            }
        }

        public DBServiceManager(DBServiceManagerBuilder builder)
        {
            builder.ConfigAdded += AddConfig;
        }

        #region IDBService
        public IDBClassResolver DBClassResolver => Default.DBClassResolver;

        public ISqlResolver SqlResolver => Default.SqlResolver;

        public IServiceProvider DBServiceProvider => Default.DBServiceProvider;

        public DBConnectionWrapper OpenConnection() => Default.OpenConnection();

        public IDBObjectMapping<T> GetDBObjectMapping<T>() => Default.GetDBObjectMapping<T>();

        public IDBTableAdapter<T> GetDBTableAdapter<T>() where T : new() => Default.GetDBTableAdapter<T>();
        #endregion

        public IDBService this[string schema]
        {
            get
            {
                if (!_DBServices.TryGetValue(schema, out IDBService svr))
                {
                    throw new Exception($"DBService for [name:{schema}] is not registered.");
                }
                return svr;
            }
        }
    }
}
