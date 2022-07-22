using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MagicEastern.ADOExt
{
    public class DBTableAdapter<T> : DBTableAdapterContext<T>, IDBTableAdapter<T> where T : new()
    {
        private readonly IDBTableAdapterCommand<T> InsertCommand = null;
        private readonly IDBTableAdapterCommand<T> UpdateCommand = null;
        private readonly IDBTableAdapterCommand<T> DeleteCommand = null;
        private readonly IDBTableAdapterCommand<T> LoadCommand = null;
        private readonly ICommandBuilder<T> CommandBuilder;

        public DBTableAdapter(ICommandBuilder<T> commandBuilder, DBConnectionWrapper currentConnection, DBTransactionWrapper currentTrans) 
            : base(currentConnection, currentTrans)
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

        public virtual int Delete(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            if (DeleteCommand == null)
            {
                throw new NotSupportedException("Delete is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }

            return DeleteCommand.Execute(obj, conn, out _, trans);
        }

        public virtual T Load(T obj, DBConnectionWrapper conn, DBTransactionWrapper trans = null)
        {
            if (LoadCommand == null)
            {
                throw new NotSupportedException("Load is not support for this [" + typeof(T).FullName + "], make sure you have primary key defined in Entity Metadata for this type.");
            }
            LoadCommand.Execute(obj, conn, out T ret, trans);
            return ret;
        }

        public virtual int Insert(T obj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null)
        {
            return InsertCommand.Execute(obj, conn, out result, trans);
        }

        private IDBTableAdapterCommand<T> CreateUpdateCommand(Expression<Func<T, object>>[] targetProperties)
        {
            var hash = new HashSet<PropertyInfo>();

            foreach (var i in targetProperties) {
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
                hash.Add(prop);
            }
           
            var setCols = SetColumnsInfo.Where(i => hash.Contains(i.ObjectProperty)).ToList();
            if (setCols.Count == 0)
            {
                throw new ArgumentException("No column will be updated!");
            }

            var updateContext = new DBTableAdapterContext<T>(this.Mapping, this.DBService
                , (List<IDBColumnMapping<T>>)AllColumnsInfo, (List<IDBColumnMapping<T>>)PkColumnsInfo, (List<IDBColumnMapping<T>>)InsertColumnsInfo, setCols);
            IDBTableAdapterCommand<T> cmd = CommandBuilder.CreateUpdateCommand(updateContext);
            return cmd;
        }

        public virtual int Update(T obj, DBConnectionWrapper conn, out T result, DBTransactionWrapper trans = null, params Expression<Func<T, object>>[] targetProperties)
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
            return cmd.Execute(obj, conn, out result, trans);
        }

    }
}
