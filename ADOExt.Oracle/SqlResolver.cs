using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.Oracle
{
    internal class SqlResolver : ISqlResolver
    {
        public string DataBaseType => ADOExt.DataBaseType.Oracle;

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

        public string DeleteTemplate(string table, IEnumerable<string> pkColumns, string schema = null)
        {
            string sql = "delete from " + GetTableName(table, schema) + " where " + string.Join(" and ", pkColumns.Select(i => i + "=:" + i));
            return sql;
        }

        //public string InsertTemplate(string table, string[] insertColumns)
        //{
        //    string sql = "insert into " + table + "(" + string.Join(",", insertColumns) + ") values (:" + string.Join(",:", insertColumns) + ")";
        //    return sql;
        //}

        public string InsertTemplate(string table, IEnumerable<string> insertColumns, IEnumerable<string> returningColumns, string schema = null)
        {
            string sql = "begin\r\n";
            sql += "insert into " + GetTableName(table, schema) + "(" + string.Join(",", insertColumns) + ") values (:" + string.Join(",:", insertColumns) + ") returning " + string.Join(",", returningColumns) + " into :" + string.Join(",:", returningColumns) + ";\r\n";
            sql += "end;";
            return sql;
        }

        public string LoadTemplate(string table, IEnumerable<string> pkColumns, IEnumerable<string> selectColumns, string schema = null)
        {
            string sql = "select " + string.Join(",", selectColumns) + " from " + GetTableName(table, schema) + " where " + string.Join(" and ", pkColumns.Select(i => i + "=:" + i));
            return sql;
        }

        //public string UpdateTemplate(string table, string[] pkColumns, string[] setColumns)
        //{
        //    string sql = "update " + table + " set " + string.Join(",", setColumns.Select(i => i + "=:" + i)) + " where " + string.Join(" and ", pkColumns.Select(i => i + "=:" + i));
        //    return sql;
        //}

        public string UpdateTemplate(string table, IEnumerable<string> pkColumns, IEnumerable<string> setColumns, IEnumerable<string> returningColumns, string schema = null)
        {
            string sql = "begin\r\n";
            sql += "update " + GetTableName(table, schema) + " set " + string.Join(",", setColumns.Select(i => i + "=:" + i)) + " where " + string.Join(" and ", pkColumns.Select(i => i + "=:" + i)) + " returning " + string.Join(",", returningColumns) + " into :" + string.Join(",:", returningColumns) + ";\r\n";
            sql += "end;";
            return sql;
        }
    }
}