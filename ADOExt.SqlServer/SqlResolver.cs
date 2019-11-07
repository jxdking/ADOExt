using System;
using System.Collections.Generic;
using System.Linq;

namespace MagicEastern.ADOExt.SqlServer
{
    internal class SqlResolver : ISqlResolver
    {
        public string DataBaseType => ADOExt.DataBaseType.SqlServer;

        public Sql ColumnMetaDataFromTable(string table, string schema = null)
        {
            if (string.IsNullOrEmpty(table))
            {
                throw new ArgumentNullException("tableName");
            }

            string sqlTxt =
                @"select a.TABLE_SCHEMA AS table_schema,
	                    a.table_name as table_name,
                        a.column_name as column_name,
                        upper(a.data_type) as data_type,
                        a.char_length,
                        a.data_precision,
                        a.data_scale,
                        a.nullable,
                        case when (b.COLUMN_NAME is null) then 'N' else 'Y' end as PK from
	                (select TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH as CHAR_LENGTH
		                ,NUMERIC_PRECISION as DATA_PRECISION, NUMERIC_SCALE AS DATA_SCALE
		                , case (IS_NULLABLE) when 'YES' then 'Y' else 'N' end AS NULLABLE
                    from INFORMATION_SCHEMA.COLUMNS
	                where upper(TABLE_NAME) = @tablename {0}) a
                left outer join
	                (SELECT Col.Column_Name from
		                INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab,
		                INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col
	                WHERE
		                Col.Constraint_Name = Tab.Constraint_Name
		                AND Col.Table_Name = Tab.Table_Name
						and Col.TABLE_SCHEMA = Tab.TABLE_SCHEMA
		                AND Constraint_Type = 'PRIMARY KEY'
		                AND upper(Col.Table_Name) = @tablename {1}) b
                on a.COLUMN_NAME = b.COLUMN_NAME";
            if (!string.IsNullOrWhiteSpace(schema))
            {
                sqlTxt = string.Format(sqlTxt, "and upper(TABLE_SCHEMA) = @tableschema", "AND upper(Col.TABLE_SCHEMA) = @tableschema");
            }
            else
            {
                sqlTxt = string.Format(sqlTxt, "", "");
            }
            Sql sql = new Sql(sqlTxt, new Parameter("tablename", table.ToUpper()), new Parameter("tableschema", schema?.ToUpper()));

            return sql;
        }

        private string GetTableName(string table, string schema)
        {
            string tablename = "[" + table + "]";
            if (!string.IsNullOrWhiteSpace(schema))
            {
                tablename = "[" + schema + "].[" + table + "]";
            }
            return tablename;
        }

        public string DeleteTemplate(string table, IEnumerable<string> pkColumns, string schema = null)
        {
            var tablename = GetTableName(table, schema);
            string sql = "delete from " + tablename + " where " + string.Join(" and ", pkColumns.Select(i => "[" + i + "]=@" + i));
            return sql;
        }

        public string InsertTemplate(string table, IEnumerable<string> insertColumns, IEnumerable<string> returningColumns, string schema = null)
        {
            var tablename = GetTableName(table, schema);
            string sql = "insert into " + tablename + "([" + string.Join("],[", insertColumns) + "]) values (@" + string.Join(",@", insertColumns) + ")";
            return sql;
        }

        public string LoadTemplate(string table, IEnumerable<string> pkColumns, IEnumerable<string> selectColumns, string schema = null)
        {
            var tablename = GetTableName(table, schema);
            string sql = "select [" + string.Join("],[", selectColumns) + "] from " + tablename + " where " + string.Join(" and ", pkColumns.Select(i => "[" + i + "]=@" + i));
            return sql;
        }

        public string UpdateTemplate(string table, IEnumerable<string> pkColumns, IEnumerable<string> setColumns, IEnumerable<string> returningColumns, string schema = null)
        {
            var tablename = GetTableName(table, schema);
            string sql = "update " + tablename + " set " + string.Join(",", setColumns.Select(i => "[" + i + "]=@" + i)) + " where " + string.Join(" and ", pkColumns.Select(i => "[" + i + "]=@" + i));
            return sql;
        }
    }
}