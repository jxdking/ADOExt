using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapter<T> : DBTableAdapterContext<T>, IDBTableAdapter<T> where T : new()
    {
        private readonly ISqlInsertTemplate<T> InsertCommand = null;
        private readonly ISqlUpdateTemplate<T> UpdateCommand = null;
        private readonly ISqlDeleteTemplate<T> DeleteCommand = null;
        private readonly ISqlLoadTemplate<T> LoadCommand = null;

        public DBTableAdapter(IDBTableMapping<T> mapping, ISqlResolver sqlResolver)
            : base(mapping)
        {
            if (PkColumnsInfo.Count > 0)
            {
                UpdateCommand = sqlResolver.GetUpdateTemplate<T>();
                DeleteCommand = sqlResolver.GetDeleteTemplate<T>();
                LoadCommand = sqlResolver.GetLoadTemplate<T>();
            }
            InsertCommand = sqlResolver.GetInsertTemplate<T>();
        }

        public virtual void ApplyMaxLength(T entity)
        {
            for (int i = 0; i < AllColumnsInfo.Count; i++)
            {
                var col = AllColumnsInfo[i];
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

        public virtual Sql GetDeleteSql(T obj, string parameterSuffix = null)
        {
            if (DeleteCommand == null)
            {
                throw new NotSupportedException("Delete is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            var sql = DeleteCommand.Generate(obj, parameterSuffix);
            return sql;
        }

        public virtual int Delete(T obj, DBConnectionWrapper conn)
        {
            var sql = GetDeleteSql(obj);
            return conn.Execute(sql, false);
        }

        private Sql GetLoadSql(T obj)
        {
            if (LoadCommand == null)
            {
                throw new NotSupportedException("Load is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            var sql = LoadCommand.Generate(obj);
            return sql;
        }

        public virtual T Load(T obj, DBConnectionWrapper conn)
        {
            var sql = GetLoadSql(obj);
            return conn.Query<T>(sql).FirstOrDefault();
        }

        private IEnumerable<IDBTableColumnMapping<T>> GetSetColumns(IEnumerable<IDBTableColumnMapping<T>> all, object properties, string operation)
        {
            IEnumerable<IDBTableColumnMapping<T>> setCols = all;
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

        public virtual Sql GetInsertSql(T obj, object properties, string parameterSuffix = null)
        {
            return InsertCommand.Generate(obj, GetSetColumns(InsertColumnsInfo, properties, "Insert()"), parameterSuffix);
        }

        public virtual int Insert(T obj, object properties, out T result, DBConnectionWrapper conn)
        {
            return InsertCommand.Execute(obj, GetSetColumns(InsertColumnsInfo, properties, "Insert()"), out result, conn);
        }

        public virtual Sql GetUpdateSql(T obj, object properties, string parameterSuffix = null)
        {
            if (UpdateCommand == null)
            {
                throw new NotSupportedException("Update is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            return UpdateCommand.Generate(obj, GetSetColumns(SetColumnsInfo, properties, "Update()"), parameterSuffix);
        }

        public virtual int Update(T obj, object properties, out T result, DBConnectionWrapper conn)
        {
            if (UpdateCommand == null)
            {
                throw new NotSupportedException("Update is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            return UpdateCommand.Execute(obj, GetSetColumns(SetColumnsInfo, properties, "Update()"), out result, conn);
        }
    }
}
