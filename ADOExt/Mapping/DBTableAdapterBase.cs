using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public abstract class DBTableAdapterBase<T> : DBTableAdapterContext<T>, IDBTableAdapter<T> where T : new()
    {
        private IDBTableAdapterCommand<T> InsertCommand = null;
        private IDBTableAdapterCommand<T> UpdateCommand = null;
        private IDBTableAdapterCommand<T> DeleteCommand = null;
        private IDBTableAdapterCommand<T> LoadCommand = null;
        private ICommandBuilder<T> CommandBuilder;

        public DBTableAdapterBase(IResolverProvider resolverProvider, ICommandBuilder<T> commandBuilder, DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) 
            : base(resolverProvider, currentConnection, currentTrans)
        {
            CommandBuilder = commandBuilder;
            InsertCommand = commandBuilder.CreateInsertCommand(this);
            UpdateCommand = commandBuilder.CreateUpdateCommand(this);
            DeleteCommand = commandBuilder.CreateDeleteCommand(this);
            LoadCommand = commandBuilder.CreateLoadCommand(this);
        }

        public virtual void ApplyMaxLength(T entity)
        {
            foreach (var ci in AllColumnsInfo)
            {
                var col = (IDBColumnMappingInfo<T>)ci;
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

        public virtual int Delete(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            if (DeleteCommand == null)
            {
                throw new NotSupportedException("Delete is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }

            return DeleteCommand.Execute(ref obj, conn, trans);
        }

        public virtual T Load(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            if (LoadCommand == null)
            {
                throw new NotSupportedException("Load is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            int cnt = LoadCommand.Execute(ref obj, conn, trans);
            if (cnt == 0) { return default(T); }
            return obj;
        }

        public virtual int Insert(ref T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            return InsertCommand.Execute(ref obj, conn, trans);
        }

        private IDBTableAdapterCommand<T> CreateUpdateCommand(Expression<Func<T, object>>[] targetProperties)
        {

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
                        if (PkColumnsInfo.FirstOrDefault(j => j.ObjectProperty == prop) != null)
                        {
                            throw new ArgumentException("Cannot update the primary key column [" + ((MemberExpression)expr).Member.Name + "]");
                        }
                        return prop;
                    }
                );
            var setCols = SetColumnsInfo.Where(i => targetProps.Contains(i.ObjectProperty)).ToList();
            if (setCols.Count() == 0)
            {
                throw new ArgumentException("No column will be updated!");
            }

            var updateContext = new DBTableAdapterContext<T>(this.Mapping, this.ResolverProvider, AllColumnsInfo, PkColumnsInfo, InsertColumnsInfo, setCols);
            IDBTableAdapterCommand<T> cmd = CommandBuilder.CreateUpdateCommand(updateContext);
            return cmd;
        }

        public virtual int Update(ref T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null, params Expression<Func<T, object>>[] targetProperties)
        {
            if (UpdateCommand == null)
            {
                throw new NotSupportedException("Update is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }

            IDBTableAdapterCommand<T> cmd = UpdateCommand;
            if (targetProperties.Length > 0)
            {
                cmd = CreateUpdateCommand(targetProperties);
            }
            return cmd.Execute(ref obj, conn, trans);
        }

    }
}