using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapter<T> : DBTableAdapterContext<T>, IDBTableAdapter<T> where T : new()
    {
        private readonly SqlInsertTemplateBase<T> InsertCommand = null;
        private readonly SqlUpdateTemplateBase<T> UpdateCommand = null;
        private readonly SqlDeleteTemplateBase<T> DeleteCommand = null;
        private readonly SqlLoadTemplateBase<T> LoadCommand = null;

        public DBTableAdapter(IDBTableMapping<T> mapping, ISqlResolver sqlResolver)
            : base(mapping)
        {
            if (PkColumns.Count > 0)
            {
                UpdateCommand = sqlResolver.GetUpdateTemplate(this);
                DeleteCommand = sqlResolver.GetDeleteTemplate(this);
                LoadCommand = sqlResolver.GetLoadTemplate(this);
            }
            InsertCommand = sqlResolver.GetInsertTemplate(this);
        }

        public virtual void ApplyMaxLength(T entity)
        {
            for (int i = 0; i < AllColumnsInfo.Count; i++)
            {
                var ci = AllColumnsInfo[i];
                var col = (IDBTableColumnMapping<T>)ci;
                PropertyInfo property = col.ObjectProperty;
                if (property.PropertyType.Equals(typeof(string)) && col.DataType != "CLOB" && col.CharLength > 0)
                {
                    if (col.PropertyGetter(entity) is string val && val.Length > col.CharLength)
                    {
                        col.PropertySetter(entity, val.Substring(0, (int)col.CharLength));
                    }
                }
            }
        }

        public virtual int Delete(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            if (DeleteCommand == null)
            {
                throw new NotSupportedException("Delete is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            var sql = DeleteCommand.Generate(obj);
            return conn.Execute(sql, false, trans);
        }

        public virtual T Load(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            if (LoadCommand == null)
            {
                throw new NotSupportedException("Load is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            var sql = LoadCommand.Generate(obj);
            return conn.Query<T>(sql, trans).FirstOrDefault();
        }

        private IEnumerable<IDBColumnMapping<T>> GetSetColumns(IEnumerable<IDBColumnMapping<T>> all, object properties, string operation)
        {
            IEnumerable<IDBColumnMapping<T>> setCols = all;
            if (properties != null)
            {
                var hash = new HashSet<string>(properties.GetType().GetProperties().Select(i => i.Name));
                setCols = InsertColumnsInfo.Where(i => hash.Remove(i.ObjectProperty.Name)).ToList();
                if (hash.Count > 0)
                {
                    throw new ArgumentOutOfRangeException($"Properties[{string.Join(",", hash)}] are not valid for {operation} operation" +
                        $" or are not defined in type [{typeof(T).FullName}].");
                }
            }
            return setCols;
        }

        public virtual int Insert(T obj, object properties, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            return InsertCommand.Execute(obj, GetSetColumns(InsertColumnsInfo, properties, "Insert()"), out result, conn, trans);
        }

        public virtual int Update(T obj, object properties, out T result, DBConnectionWrapper conn, DBTransactionWrapper trans)
        {
            if (UpdateCommand == null)
            {
                throw new NotSupportedException("Update is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            return UpdateCommand.Execute(obj, GetSetColumns(SetColumnsInfo, properties, "Update()"), out result, conn, trans);
        }

    }
}
