using System;

namespace MagicEastern.ADOExt.Oracle
{
    public class SqlResolver : ISqlResolver
    {
        internal string GetTableName(string table, string schema)
        {
            string tablename = table;
            if (!string.IsNullOrWhiteSpace(schema))
            {
                tablename = schema + "." + table;
            }
            return tablename;
        }

        public Sql ColumnMetaDataFromTable(string table, string schema)
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
                return new Sql(sqltxt, new Parameter { Name = "tablename", Value = table.ToUpper() }, new Parameter { Name = "tableschema", Value = schema?.ToUpper() });
            }

            sqltxt = string.Format(sqltxt, "", "");
            return new Sql(sqltxt, new Parameter { Name = "tablename", Value = table.ToUpper() });
        }

        public SqlInsertTemplateBase<T> GetInsertTemplate<T>(DBTableAdapterContext<T> context) where T : new()
        {
            return new SqlInsertTemplate<T>(context, this);
        }

        public SqlUpdateTemplateBase<T> GetUpdateTemplate<T>(DBTableAdapterContext<T> context) where T : new()
        {
            return new SqlUpdateTemplate<T>(context, this);
        }

        public SqlLoadTemplateBase<T> GetLoadTemplate<T>(DBTableAdapterContext<T> context) where T : new()
        {
            return new SqlLoadTemplate<T>(context, this);
        }

        public SqlDeleteTemplateBase<T> GetDeleteTemplate<T>(DBTableAdapterContext<T> context) where T : new()
        {
            return new SqlDeleteTemplate<T>(context, this);
        }
    }
}