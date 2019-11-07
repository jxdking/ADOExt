namespace MagicEastern.ADOExt
{
    public class DBColumnMappingInfo<T> : DBColumnMapping<T>, IDBColumnMappingInfo<T>
    {
        public string TableName { get; protected set; }
        public string DataType { get; protected set; }
        public int? CharLength { get; protected set; }
        public int? DataPrecision { get; protected set; }
        public int? DataScale { get; protected set; }
        public bool PK { get; protected set; }

        public DBColumnMappingInfo(IDBColumnMapping<T> mapping, SchemaMetadata metadata)
        {
            ColumnName = mapping.ColumnName;
            NoInsert = mapping.NoInsert;
            Required = mapping.Required;
            ObjectProperty = mapping.ObjectProperty;
            PropertyGetter = mapping.PropertyGetter;
            //PropertySetter = mapping.PropertySetter;

            ColumnName = metadata.COLUMN_NAME;
            TableName = metadata.TABLE_NAME;
            DataType = metadata.DATA_TYPE;
            CharLength = metadata.CHAR_LENGTH;
            DataPrecision = metadata.DATA_PRECISION;
            DataScale = metadata.DATA_SCALE;
            Required = Required || metadata.NULLABLE == "N";
            PK = metadata.PK == "Y";

            // recreate setter as the Required condition may be changed.
            PropertySetter = CreateSetter(ObjectProperty, !Required);
        }
    }
}