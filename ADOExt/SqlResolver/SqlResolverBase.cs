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

        public SqlDeleteTemplateBase<T> GetDeleteTemplate<T>() where T : new()
        {
            return DBServiceProvider.GetService<SqlDeleteTemplateBase<T>>();
        }

        public SqlInsertTemplateBase<T> GetInsertTemplate<T>() where T : new()
        {
            return DBServiceProvider.GetService<SqlInsertTemplateBase<T>>();
        }

        public SqlLoadTemplateBase<T> GetLoadTemplate<T>() where T : new()
        {
            return DBServiceProvider.GetService<SqlLoadTemplateBase<T>>();
        }

        public SqlUpdateTemplateBase<T> GetUpdateTemplate<T>() where T : new()
        {
            return DBServiceProvider.GetService<SqlUpdateTemplateBase<T>>();
        }
    }
}
