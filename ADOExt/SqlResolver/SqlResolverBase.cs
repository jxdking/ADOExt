using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;


namespace MagicEastern.ADOExt
{
    public abstract class SqlResolverBase : ISqlResolver
    {
        private readonly IServiceProvider DBServiceProvider;

        public SqlResolverBase(IServiceProvider sp)
        {
            this.DBServiceProvider = sp;
        }

        public abstract Sql ColumnMetaDataFromTable(string table, string schema);
        public abstract string GetTableName(string table, string schema);

        public ISqlDeleteTemplate<T> GetDeleteTemplate<T>() where T : new()
        {
            return DBServiceProvider.GetService<ISqlDeleteTemplate<T>>();
        }

        public ISqlInsertTemplate<T> GetInsertTemplate<T>() where T : new()
        {
            return DBServiceProvider.GetService<ISqlInsertTemplate<T>>();
        }

        public ISqlLoadTemplate<T> GetLoadTemplate<T>() where T : new()
        {
            return DBServiceProvider.GetService<ISqlLoadTemplate<T>>();
        }

        public ISqlUpdateTemplate<T> GetUpdateTemplate<T>() where T : new()
        {
            return DBServiceProvider.GetService<ISqlUpdateTemplate<T>>();
        }
    }
}
