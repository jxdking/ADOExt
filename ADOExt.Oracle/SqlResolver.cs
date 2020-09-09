using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.Oracle
{
    internal class SqlResolver : ISqlResolver
    {
        
        private string GetTableName(string table, string schema)
        {
            string tablename = table;
            if (!string.IsNullOrWhiteSpace(schema))
            {
                tablename = schema + "." + table;
            }
            return tablename;
        }

        public Sql ColumnMetaDataFromTable(string table, string schema = null)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("tableName");
            }

            string sqltxt =
                @"select nvl2(p.column_name, 'Y', 'N') pk, c.owner as table_schema, c.table_name, c.column_name, c.data_type, c.char_length, c.data_precision, c.data_scale, c.nullable
                from all_tab_cols c
                    left outer join (
                        select b.column_name
                        from all_constraints a
                            join ALL_CONS_COLUMNS b
                            on (a.constraint_name = b.constraint_name
                              and a.table_name = b.table_name
                              and a.owner = b.owner)
                        where upper(a.table_name) = :tablename {0}
                        and a.constraint_type = 'P') p
                    on (c.column_name = p.column_name)
                where upper(c.table_name) = :tablename {1}";

            if (!string.IsNullOrWhiteSpace(schema))
            {
                sqltxt = string.Format(sqltxt, "and upper(a.owner) = :tableschema", "and upper(c.owner) = :tableschema");
            }
            else
            {
                sqltxt = string.Format(sqltxt, "", "");
            }

            return new Sql(sqltxt, new Parameter("tablename", table.ToUpper()), new Parameter("tableschema", schema?.ToUpper()));
        }

        private IEnumerable<string> GetReturningCols<T>(DBTableAdapterContext<T> context)
        {
            var returningCols = context.Mapping.ColumnMappingList.Where(i => i.DataType != "LONG").Select(i => i.ColumnName);
            return returningCols;
        }

        public string InsertTemplate<T>(DBTableAdapterContext<T> context)
        {
            var returningCols = GetReturningCols(context);
            string sql = "begin\r\n";
            sql += "insert into " + GetTableName(context.Mapping.TableName, context.Mapping.Schema) + "(" + string.Join(",", context.InsertColumns) + ") values (:" + string.Join(",:", context.InsertColumns) + ") returning " + string.Join(",", returningCols) + " into :" + string.Join(",:", returningCols) + ";\r\n";
            sql += ":sql_nor := sql%ROWCOUNT;\r\n";
            sql += "end;";
            return sql;
        }

        public string DeleteTemplate<T>(DBTableAdapterContext<T> context)
        {
            string sql = "delete from " + GetTableName(context.Mapping.TableName, context.Mapping.Schema) + " where " + string.Join(" and ", context.PkColumns.Select(i => i + "=:" + i));
            return sql;
        }

        public string UpdateTemplate<T>(DBTableAdapterContext<T> context)
        {
            var returningCols = GetReturningCols(context);
            string sql = "begin\r\n";
            sql += "update " + GetTableName(context.Mapping.TableName, context.Mapping.Schema) + " set " + string.Join(",", context.SetColumns.Select(i => i + "=:" + i)) + " where " + string.Join(" and ", context.PkColumns.Select(i => i + "=:" + i)) + " returning " + string.Join(",", returningCols) + " into :" + string.Join(",:", returningCols) + ";\r\n";
            sql += ":sql_nor := sql%ROWCOUNT;\r\n";
            sql += "end;";
            return sql;
        }

        public string LoadTemplate<T>(DBTableAdapterContext<T> context)
        {
            string sql = "select " + string.Join(",", context.AllColumns) + " from " + GetTableName(context.Mapping.TableName, context.Mapping.Schema) + " where " + string.Join(" and ", context.PkColumns.Select(i => i + "=:" + i));
            return sql;
        }
    }
}