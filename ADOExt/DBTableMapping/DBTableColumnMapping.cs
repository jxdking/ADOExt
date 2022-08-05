namespace MagicEastern.ADOExt
{
    public class DBTableColumnMapping<T> : DBColumnMapping<T>, IDBTableColumnMapping<T>
    {
        public string TableName { get; protected set; }
        public string DataType { get; protected set; }
        public int? CharLength { get; protected set; }
        public int? DataPrecision { get; protected set; }
        public int? DataScale { get; protected set; }
        public bool PK { get; protected set; }

        public DBTableColumnMapping(IDBColumnMapping<T> mapping, DBTableMetadata metadata)
        {
            DbType = mapping.DbType;
            ColumnName = mapping.ColumnName;
            NoInsert = mapping.NoInsert;
            NoUpdate = mapping.NoUpdate;
            Required = mapping.Required;
            ObjectProperty = mapping.ObjectProperty;
            PropertyGetter = mapping.PropertyGetter;
            PropertySetter = mapping.PropertySetter;

            ColumnName = metadata.COLUMN_NAME;
            TableName = metadata.TABLE_NAME;
            DataType = metadata.DATA_TYPE;
            CharLength = metadata.CHAR_LENGTH;
            DataPrecision = metadata.DATA_PRECISION;
            DataScale = metadata.DATA_SCALE;
            Required = Required || metadata.NULLABLE == "N";
            PK = metadata.PK == "Y";
        }
    }
}