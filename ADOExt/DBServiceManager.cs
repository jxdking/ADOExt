using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MagicEastern.ADOExt
{
    public class DBServiceManager : IDBService
    {
        private readonly Dictionary<string, IServiceProvider> _ServiceProvider;
        private IDBService Default = null;

        private void AddConfig(object sender, ConfigAddedEventArgs args)
        {
            _ServiceProvider[args.ConfigName] = args.ServiceProvider;
            if (Default == null)
            {
                Default = args.ServiceProvider.GetService<IDBService>();
            }
        }

        public DBServiceManager(DBServiceManagerBuilder builder)
        {
            _ServiceProvider = new Dictionary<string, IServiceProvider>();
            builder.ConfigAdded += AddConfig;
        }

        #region IDBService
        public IDBClassResolver DBClassResolver => Default.DBClassResolver;

        public ISqlResolver SqlResolver => Default.SqlResolver;

        public IDBObjectMappingFactory DBObjectMappingFactory => Default.DBObjectMappingFactory;

        public IDBTableMappingFactory DBTableMappingFactory => Default.DBTableMappingFactory;

        public IDBTableAdapterFactory DBTableAdapterFactory => Default.DBTableAdapterFactory;
        #endregion

        public IDBService this[string schema]
        {
            get
            {
                if (!_ServiceProvider.TryGetValue(schema, out IServiceProvider sp))
                {
                    throw new Exception($"DBService for [name:{schema}] is not registered.");
                }
                return sp.GetService<IDBService>();
            }
        }
    }
}
