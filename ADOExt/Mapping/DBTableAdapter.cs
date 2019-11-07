using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapter<T> where T : new()
    {
        private Command<T> InsertCommand = null;
        private Command<T> UpdateCommand = null;
        private Command<T> DeleteCommand = null;
        private Command<T> LoadCommand = null;

        private IResolverProvider ResolverProvider = null;
        private DBTableMapping<T> Mapping = null;

        private List<string> insertColumns = new List<string>();
        private List<string> allColumns = new List<string>();
        private List<IDBColumnMapping<T>> allColumnsInfo = new List<IDBColumnMapping<T>>();
        private List<string> pkColumns = new List<string>();
        private List<IDBColumnMapping<T>> pkColumnsInfo = new List<IDBColumnMapping<T>>();
        private List<string> setColumns = new List<string>();
        private List<IDBColumnMapping<T>> setColumnsInfo = new List<IDBColumnMapping<T>>();

        private static DBTableAdapter<T>[] _Cache = new DBTableAdapter<T>[Constant.MaxResolverProvidorIdx];

        public static DBTableAdapter<T> Get(IResolverProvider resolverProvider)
        {
            var ret = _Cache[resolverProvider.Idx];
            if (ret == null)
            {
                return _Cache[resolverProvider.Idx] = new DBTableAdapter<T>(resolverProvider);
            }
            return ret;
        }

        private DBTableAdapter(IResolverProvider resolverProvider) : this(resolverProvider, DBTableMapping<T>.Get(resolverProvider))
        { }

        private DBTableAdapter(IResolverProvider resolverProvider, DBTableMapping<T> mapping)
        {
            ResolverProvider = resolverProvider;
            Mapping = mapping;

            foreach (var c in mapping.ColumnMappingList)
            {
                if (!c.NoInsert) { insertColumns.Add(c.ColumnName); }
                allColumns.Add(c.ColumnName);
                allColumnsInfo.Add(c);
                if (c.PK) { pkColumns.Add(c.ColumnName); pkColumnsInfo.Add(c); }
                else { setColumns.Add(c.ColumnName); setColumnsInfo.Add(c); }
            }

            InsertCommand = new Command<T>();
            InsertCommand.CommandText = resolverProvider.SqlResolver.InsertTemplate(mapping.TableName, insertColumns, allColumns, mapping.Schema);
            InsertCommand.ColumnInfoList = allColumnsInfo;

            if (pkColumns.Count > 0)
            {
                DeleteCommand = new Command<T>();
                DeleteCommand.CommandText = resolverProvider.SqlResolver.DeleteTemplate(mapping.TableName, pkColumns, mapping.Schema);
                DeleteCommand.ColumnInfoList = pkColumnsInfo;

                UpdateCommand = new Command<T>();
                UpdateCommand.CommandText = resolverProvider.SqlResolver.UpdateTemplate(mapping.TableName, pkColumns, setColumns, allColumns, mapping.Schema);
                UpdateCommand.ColumnInfoList = allColumnsInfo;

                LoadCommand = new Command<T>();
                LoadCommand.CommandText = resolverProvider.SqlResolver.LoadTemplate(mapping.TableName, pkColumns, allColumns, mapping.Schema);
                LoadCommand.ColumnInfoList = pkColumnsInfo;
            }
        }

        public int Delete(T obj, IDbConnection conn, IDbTransaction trans = null)
        {
            if (DeleteCommand == null)
            {
                throw new NotSupportedException("Delete is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }

            return DeleteCommand.Execute(ref obj, conn, trans);
        }

        public T Load(T obj, IDbConnection conn, IDbTransaction trans = null)
        {
            if (LoadCommand == null)
            {
                throw new NotSupportedException("Load is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            return LoadCommand.Load(obj, conn, trans);
        }

        public int Insert(ref T obj, IDbConnection conn, IDbTransaction trans = null)
        {
            return InsertCommand.Execute(ref obj, conn, trans);
        }

        private Command<T> CreateUpdateCommand(Expression<Func<T, object>>[] targetProperties)
        {
            Command<T> cmd = new Command<T>();
            IEnumerable<PropertyInfo> targetProps = targetProperties.Select(
                    i =>
                    {
                        Expression expr = i.Body;
                        if (expr.NodeType == ExpressionType.Convert)
                        {
                            expr = ((UnaryExpression)expr).Operand;
                        }
                        if (expr.NodeType != ExpressionType.MemberAccess)
                        {
                            throw new ArgumentException("Only MemberExpression can be parsed!");
                        }
                        var prop = ((MemberExpression)expr).Member as PropertyInfo;
                        if (prop == null)
                        {
                            throw new ArgumentException("[" + ((MemberExpression)expr).Member.Name + "] is not a Property of " + typeof(T).FullName);
                        }
                        return prop;
                    }
                );
            var setCols = setColumnsInfo.Where(i => targetProps.Contains(i.ObjectProperty)).Select(i => i.ColumnName);
            if (setCols.Count() == 0)
            {
                throw new ArgumentException("No column will be updated!");
            }

            cmd.CommandText = ResolverProvider.SqlResolver.UpdateTemplate(Mapping.TableName, pkColumns, setCols, allColumns, Mapping.Schema);
            cmd.ColumnInfoList = allColumnsInfo;

            return cmd;
        }

        public int Update(ref T obj, IDbConnection conn, IDbTransaction trans = null, params Expression<Func<T, object>>[] targetProperties)
        {
            if (UpdateCommand == null)
            {
                throw new NotSupportedException("Update is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }

            Command<T> cmd = UpdateCommand;
            if (targetProperties.Length > 0)
            {
                cmd = CreateUpdateCommand(targetProperties);
            }
            return cmd.Execute(ref obj, conn, trans);
        }

        public void ApplyMaxLength(T entity)
        {
            for (int i = 0; i < allColumnsInfo.Count; i++)
            {
                var col = (IDBColumnMappingInfo<T>)allColumnsInfo[i];
                PropertyInfo property = col.ObjectProperty;
                if (property.PropertyType.Equals(typeof(string)) && col.DataType != "CLOB" && col.CharLength > 0)
                {
                    string val = col.PropertyGetter(entity) as string;
                    if (val != null && val.Length > col.CharLength)
                    {
                        col.PropertySetter(entity, val.Substring(0, (int)col.CharLength));
                    }
                }
            }
        }
    }
}