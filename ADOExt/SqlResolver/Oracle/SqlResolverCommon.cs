using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace MagicEastern.ADOExt.Common.Oracle
{
    public class SqlResolverCommon<TParameter> : SqlResolverBase
        where TParameter : IDbDataParameter, new()
    {

        public SqlResolverCommon(IServiceProvider sp) : base(sp)
        {
        }

        public override string GetTableName(string table, string schema)
        {
            string tablename = table;
            if (!string.IsNullOrWhiteSpace(schema))
            {
                tablename = schema + "." + table;
            }
            return tablename;
        }

        public override Sql ColumnMetaDataFromTable(string table, string schema)
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
                return new Sql(sqltxt, new TParameter { ParameterName = "tablename", Value = table.ToUpper() }, new TParameter { ParameterName = "tableschema", Value = schema?.ToUpper() });
            }

            sqltxt = string.Format(sqltxt, "", "");
            return new Sql(sqltxt, new TParameter { ParameterName = "tablename", Value = table.ToUpper() });
        }

       
    }


}