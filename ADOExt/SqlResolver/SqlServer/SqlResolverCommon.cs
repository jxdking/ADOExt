using System;
using System.Data;

namespace MagicEastern.ADOExt.Common.SqlServer
{
    public class SqlResolverCommon<TParameter> : SqlResolverBase
        where TParameter : IDbDataParameter, new()
    {
        public SqlResolverCommon(IServiceProvider sp) : base(sp)
        { }

        public override Sql ColumnMetaDataFromTable(string table, string schema)
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
                return new Sql(sqlTxt, new TParameter { ParameterName = "tablename", Value = table.ToUpper() }, new TParameter { ParameterName = "tableschema", Value = schema?.ToUpper() });
            }

            sqlTxt = string.Format(sqlTxt, "", "");
            return new Sql(sqlTxt, new TParameter { ParameterName = "tablename", Value = table.ToUpper() });
        }


        public override string GetTableName(string table, string schema)
        {
            string tablename = "[" + table + "]";
            if (!string.IsNullOrWhiteSpace(schema))
            {
                tablename = "[" + schema + "].[" + table + "]";
            }
            return tablename;
        }
    }
}