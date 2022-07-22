using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace MagicEastern.ADOExt
{
    public class DBServiceManager : IDBService
    {
        private readonly Dictionary<string, IServiceProvider> _ServiceProvider;
        private readonly string DefaultSchema;
        
        public DBServiceManager(IEnumerable<DBServiceConfig> configs)
        {
            _ServiceProvider = new Dictionary<string, IServiceProvider>();
            foreach (var config in configs) {
                if (config.Name == null) {
                    throw new ArgumentNullException("[config.Name] should not be null.");
                }
                if (config.ConfigServices == null) {
                    throw new ArgumentNullException("[config.ConfigServices] should not be null.");
                }
                if (_ServiceProvider.ContainsKey(config.Name)) {
                    throw new ArgumentException($"[config.Name:{config.Name}] is already defined.");
                }
                var sc = new ServiceCollection();
                config.ConfigServices(sc);
                _ServiceProvider[config.Name] = sc.BuildServiceProvider(); 
            }
            if (_ServiceProvider.Count() == 0)
            {
                throw new ArgumentNullException("[configs] should not be empty.");
            }
            DefaultSchema = configs.First().Name;
        }

        #region IDBService
        public IDBClassResolver DBClassResolver => this[DefaultSchema].DBClassResolver;

        public ISqlResolver SqlResolver => this[DefaultSchema].SqlResolver;

        public IDBObjectMappingFactory DBObjectMappingFactory => this[DefaultSchema].DBObjectMappingFactory;

        public IDBTableMappingFactory DBTableMappingFactory => this[DefaultSchema].DBTableMappingFactory;

        public IDBTableAdapterFactory DBTableAdapterFactory => this[DefaultSchema].DBTableAdapterFactory;
        #endregion

        public IDBService this[string schema] {
            get {
                if (!_ServiceProvider.TryGetValue(schema, out IServiceProvider sp)) {
                    throw new Exception($"DBService for [name:{schema}] is not registered.");
                }
                return sp.GetService<IDBService>();
            }
        }
    }
}
